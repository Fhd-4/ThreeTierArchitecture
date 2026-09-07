using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<List<ChatMessage>> GetHistoryAsync()
    {
        return await _context.ChatMessages
            .Include(m => m.Sender)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<List<ChatMessage>> GetHistoryWithReactionsAsync()
    {
        return await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Reactions)
                .ThenInclude(r => r.User)
            .Include(m => m.ReplyToMessage)
                .ThenInclude(r => r!.Sender)
            .Where(m => m.ReceiverId == null)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<ChatMessage?> GetMessageByIdAsync(int messageId)
    {
        return await _context.ChatMessages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId);
    }

    public async Task UpdateMessageAsync(ChatMessage message)
    {
        _context.ChatMessages.Update(message);
        await _context.SaveChangesAsync();
    }

    public async Task<MessageReaction?> GetReactionAsync(int messageId, string userId)
    {
        return await _context.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);
    }

    public async Task AddReactionAsync(MessageReaction reaction)
    {
        await _context.MessageReactions.AddAsync(reaction);
    }

    public async Task DeleteReactionAsync(MessageReaction reaction)
    {
        _context.MessageReactions.Remove(reaction);
        await Task.CompletedTask;
    }

    public async Task<bool> HasReadStateAsync(int messageId, string userId)
    {
        return await _context.MessageReadStates
            .AnyAsync(r => r.MessageId == messageId && r.UserId == userId);
    }

    public async Task AddReadStateAsync(MessageReadState readState)
    {
        await _context.MessageReadStates.AddAsync(readState);
    }

    public async Task<bool> MessageExistsAsync(int messageId)
    {
        return await _context.ChatMessages.AnyAsync(m => m.Id == messageId);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}