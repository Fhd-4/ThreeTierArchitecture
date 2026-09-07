using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface IChatRepository
{
    Task<ChatMessage> AddMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetHistoryAsync();
    Task<List<ChatMessage>> GetHistoryWithReactionsAsync();
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<ChatMessage?> GetMessageByIdAsync(int messageId);
    Task UpdateMessageAsync(ChatMessage message);

    // Reactions
    Task<MessageReaction?> GetReactionAsync(int messageId, string userId);
    Task AddReactionAsync(MessageReaction reaction);
    Task DeleteReactionAsync(MessageReaction reaction);

    // Read states
    Task<bool> HasReadStateAsync(int messageId, string userId);
    Task AddReadStateAsync(MessageReadState readState);
    Task<bool> MessageExistsAsync(int messageId);
    
    Task<bool> SaveChangesAsync();
}