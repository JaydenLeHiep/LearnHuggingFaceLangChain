using System.Text;

namespace LearnHuggingFaceLangChainApp1.LLM;
using Microsoft.ML.OnnxRuntimeGenAI;

public class OnnxModelClient : IModelClient, IDisposable
{
    private readonly Model _model;
    private readonly Tokenizer _tokenizer;

    public OnnxModelClient(string modelPath)
    {
        _model = new Model(modelPath);
        _tokenizer = new Tokenizer(_model);
    }
    
    public Task<string> GenerateAsync(string prompt, int maxNewTokens = 1024, float temperature = 0.7f, float topP = 0.95f,
        CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            using var gen = new GeneratorParams(_model);

            // Use a generous ceiling; Phi-3 mini context is ~4k, but stay conservative.
            gen.SetSearchOption("max_length", 2048);
            gen.SetSearchOption("temperature", temperature);
            gen.SetSearchOption("top_p", topP);

            var input = _tokenizer.Encode(prompt);

            using var g = new Generator(_model, gen);
            g.AppendTokenSequences(input);

            var sb = new StringBuilder();
            using var stream = _tokenizer.CreateStream();
            while (!g.IsDone())
            {
                g.GenerateNextToken();
                var seq = g.GetSequence(0);
                sb.Append(stream.Decode(seq[^1]));
            }

            // Safety net to avoid the model “writing the next user turn”
            var text = TrimAtMarkers(sb.ToString(), new[] { "<|user|>", "\nUser:", "**Instruction:**" });
            return text;

        }, ct);
    }
    
    private static string TrimAtMarkers(string s, string[] markers)
    {
        foreach (var m in markers)
        {
            var i = s.IndexOf(m, StringComparison.Ordinal);
            if (i >= 0) return s[..i];
        }
        return s;
    }
    public void Dispose()
    {
        _tokenizer?.Dispose();
        _model?.Dispose();
    }

}