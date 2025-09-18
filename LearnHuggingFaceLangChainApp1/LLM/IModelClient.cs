namespace LearnHuggingFaceLangChainApp1.LLM;

public interface IModelClient
{
    Task<string> GenerateAsync(string prompt, int maxNewTokens = 256, float temperature = 0.7f, float topP = 0.95f,
        CancellationToken ct = default);
}