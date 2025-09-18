

using LearnHuggingFaceLangChainApp1.Models;

namespace LearnHuggingFaceLangChainApp1.Repositories;

public interface IChatHistoryRepository
{
    Task AppendAsync(ChatMessage msg, CancellationToken ct = default);
    Task<List<ChatMessage>> GetHistoryAsync(string sessionId, int limit = 20, CancellationToken ct = default);
}