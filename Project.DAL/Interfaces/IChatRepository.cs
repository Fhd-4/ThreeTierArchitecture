using Project.DAL.Entities;

namespace Project.DAL.Interfaces;

public interface IChatRepository
{
    Task<ChatMessage> AddMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetHistoryAsync();
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
}