using LearnHuggingFaceLangChainApp1.Data;
using LearnHuggingFaceLangChainApp1.Models;
using Microsoft.EntityFrameworkCore;


namespace LearnHuggingFaceLangChainApp1.Repositories;

public class ChatHistoryRepository : IChatHistoryRepository
{
    private readonly ApplicationDbContext _db;

    public ChatHistoryRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    
    public async Task AppendAsync(ChatMessage msg, CancellationToken ct = default)
    {
        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync(ct);
    }
    

    public async Task<List<ChatMessage>> GetHistoryAsync(string sessionId, int limit = 20, CancellationToken ct = default)
    {
        return await _db.ChatMessages
            .Where(c => c.SessionId == sessionId)
            .OrderBy(c => c.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }
}