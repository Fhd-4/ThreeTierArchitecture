using Project.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public interface IChatService
{
    Task<IEnumerable<object>> GetChatHistoryAsync();
    Task<object?> AddMessageAsync(string senderId, string content, int? replyToMessageId, string? fileUrl, string? fileName, string? fileType);
    Task<object?> EditMessageAsync(int messageId, string userId, string newContent);
    Task<bool> DeleteMessageAsync(int messageId, string userId);
    Task<IEnumerable<object>> GetOnlineUsersAsync(Func<string, bool> isUserOnline, Func<string, DateTime?> getLastSeen, Func<string, DateTime?> getLastActivity);
    Task<(bool Success, bool IsRemoved, string UserName)> SendReactionAsync(int messageId, string userId, string emoji);
    Task<(bool Success, DateTime ReadAt)> MarkAsReadAsync(int messageId, string userId);
}
