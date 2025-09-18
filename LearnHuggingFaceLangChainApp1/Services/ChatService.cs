using System.Text;
using LearnHuggingFaceLangChainApp1.LLM;
using LearnHuggingFaceLangChainApp1.Models;
using LearnHuggingFaceLangChainApp1.Repositories;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace LearnHuggingFaceLangChainApp1.Services;

public class ChatService
{
    private readonly IChatHistoryRepository _historyRepository;
    private readonly IModelClient _modelClient;

    public ChatService(IChatHistoryRepository historyRepository, IModelClient modelClient)
    {
        _historyRepository = historyRepository;
        _modelClient = modelClient;
    }

    public async Task<string> SendAsync(string sessionId, string userText, CancellationToken ct = default)
    {
        // save user message 
        await AppendRoleAsync(sessionId, ChatRole.User, userText, DateTime.UtcNow);

        // load a generous slice once
        var all = await _historyRepository.GetHistoryAsync(sessionId, limit: 50, ct);

        // try shrinking windows to avoid “exceeds max length”
        var windows = new[] { 8, 6, 4 };
        string? reply = null;
        Exception? lastError = null;

        foreach (var w in windows)
        {
            var history = all.Count > w ? all.Skip(all.Count - w).ToList() : all;
            var prompt = BuildPhi3Prompt(history);

            try
            {
                reply = await _modelClient.GenerateAsync(prompt, maxNewTokens: 256, temperature: 0.7f, topP: 0.95f, ct);
                break;
            }
            catch (OnnxRuntimeGenAIException ex) when (ex.Message.Contains("exceeds max length", StringComparison.OrdinalIgnoreCase))
            {
                lastError = ex;
            }
        }

        if (reply is null && lastError is not null) throw lastError;
        
        // save assistant message
        await AppendRoleAsync(sessionId, ChatRole.Assistant, reply, DateTime.UtcNow);

        return reply;
    }
    
    private async Task AppendRoleAsync(string sessionId, ChatRole role, string content, DateTime createdAt)
    {
        await _historyRepository.AppendAsync(new ChatMessage
        {
            SessionId = sessionId,
            Role = role,
            Content = content,
            CreatedAt = createdAt
        });
    }

    private static string BuildPrompt(List<ChatMessage> msgs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a helpful assistant");

        foreach (var m in msgs)
        {
            if (m.Role == ChatRole.User)
            {
                sb.AppendLine($"User: {m.Content}");
            }
            else if (m.Role == ChatRole.Assistant)
            {
                sb.AppendLine($"Assistant: {m.Content}");
            }
        }

        sb.Append("Assistant: ");
        return sb.ToString();
    }
    private static string BuildPhi3Prompt(List<ChatMessage> msgs)
    {
        // Minimal Phi-3 chat template
        // <|system|>...<|end|><|user|>...<|end|><|assistant|>...
        var sb = new StringBuilder();
        sb.Append("<|system|>You are a helpful assistant.<|end|>");

        foreach (var m in msgs)
        {
            if (m.Role == ChatRole.User)
            {
                sb.Append("<|user|>").Append(m.Content).Append("<|end|>");
            }
            else if (m.Role == ChatRole.Assistant)
            {
                sb.Append("<|assistant|>").Append(m.Content).Append("<|end|>");
            }
        }

        // The model should now complete the assistant turn
        sb.Append("<|assistant|>");
        return sb.ToString();
    }

}