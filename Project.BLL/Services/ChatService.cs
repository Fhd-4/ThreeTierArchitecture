using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _repo;

    public ChatService(IChatRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<object>> GetChatHistoryAsync()
    {
        var messages = await _repo.GetHistoryWithReactionsAsync();
        return messages.Select(m => new
        {
            m.Id,
            m.SenderId,
            SenderName = m.Sender != null
                ? (m.Sender.NameAr ?? m.Sender.UserName ?? "Unknown")
                : "Unknown",
            SenderPhoto = string.IsNullOrEmpty(m.Sender?.ProfilePhoto)
                ? "/images/default-profile.png"
                : m.Sender.ProfilePhoto,
            m.Content,
            m.FileUrl,
            m.FileName,
            m.FileType,
            m.ReplyToMessageId,
            m.Timestamp,
            ReplyToMessage = m.ReplyToMessage != null ? new
            {
                m.ReplyToMessage.Id,
                m.ReplyToMessage.SenderId,
                SenderName = m.ReplyToMessage.Sender != null 
                    ? (m.ReplyToMessage.Sender.NameAr ?? m.ReplyToMessage.Sender.UserName ?? "Unknown") 
                    : "Unknown",
                m.ReplyToMessage.Content,
                m.ReplyToMessage.FileUrl,
                m.ReplyToMessage.FileName,
                m.ReplyToMessage.FileType,
                m.ReplyToMessage.Timestamp
            } : null,
            Reactions = m.Reactions.Select(r => new
            {
                r.Id,
                r.MessageId,
                r.UserId,
                UserName = r.User != null ? (r.User.NameAr ?? r.User.UserName ?? "Unknown") : "Unknown",
                r.Emoji,
                r.Timestamp
            }).ToList(),
            m.IsEdited,
            m.EditedAt,
            m.IsDeleted,
            m.DeletedAt
        });
    }

    public async Task<object?> AddMessageAsync(string senderId, string content, int? replyToMessageId, string? fileUrl, string? fileName, string? fileType)
    {
        var sender = await _repo.GetUserByIdAsync(senderId);
        if (sender == null) return null;

        var message = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = null,
            Content = (content ?? string.Empty).Trim(),
            ReplyToMessageId = replyToMessageId,
            FileUrl = fileUrl,
            FileName = fileName,
            FileType = fileType,
            Timestamp = DateTime.UtcNow
        };

        await _repo.AddMessageAsync(message);

        return new
        {
            message.Id,
            message.SenderId,
            SenderName = sender.NameAr ?? sender.UserName ?? "Unknown",
            SenderPhoto = string.IsNullOrEmpty(sender.ProfilePhoto)
                ? "/images/default-profile.png"
                : sender.ProfilePhoto,
            message.ReceiverId,
            ReceiverName = (string?)null,
            message.Content,
            message.FileUrl,
            message.FileName,
            message.FileType,
            message.ReplyToMessageId,
            message.Timestamp
        };
    }

    public async Task<object?> EditMessageAsync(int messageId, string userId, string newContent)
    {
        var message = await _repo.GetMessageByIdAsync(messageId);
        if (message == null || message.SenderId != userId || message.IsDeleted)
            return null;

        message.Content = newContent.Trim();
        message.IsEdited = true;
        message.EditedAt = DateTime.UtcNow;

        await _repo.UpdateMessageAsync(message);

        return new
        {
            messageId = message.Id,
            newContent = message.Content,
            isEdited = message.IsEdited,
            editedAt = message.EditedAt
        };
    }

    public async Task<bool> DeleteMessageAsync(int messageId, string userId)
    {
        var message = await _repo.GetMessageByIdAsync(messageId);
        if (message == null || message.SenderId != userId || message.IsDeleted)
            return false;

        message.IsDeleted = true;
        message.DeletedAt = DateTime.UtcNow;
        message.Content = "تم حذف الرسالة";
        message.FileUrl = null;
        message.FileName = null;
        message.FileType = null;

        await _repo.UpdateMessageAsync(message);
        return true;
    }

    public async Task<IEnumerable<object>> GetOnlineUsersAsync(Func<string, bool> isUserOnline, Func<string, DateTime?> getLastSeen, Func<string, DateTime?> getLastActivity)
    {
        var chatUsers = await _repo.GetAllUsersAsync();

        return chatUsers.Select(u =>
        {
            var isOnline = isUserOnline(u.Id);
            var lastSeen = !isOnline ? getLastSeen(u.Id) : null;
            var lastActivity = getLastActivity(u.Id);

            return new
            {
                userId = u.Id,
                userName = u.NameAr ?? u.UserName ?? "Unknown",
                profilePhoto = string.IsNullOrEmpty(u.ProfilePhoto)
                    ? "/images/default-profile.png"
                    : u.ProfilePhoto,
                isOnline = isOnline,
                lastSeen = lastSeen,
                lastActivity = lastActivity
            };
        });
    }

    public async Task<(bool Success, bool IsRemoved, string UserName)> SendReactionAsync(int messageId, string userId, string emoji)
    {
        var user = await _repo.GetUserByIdAsync(userId);
        if (user == null) return (false, false, string.Empty);

        var messageExists = await _repo.MessageExistsAsync(messageId);
        if (!messageExists) return (false, false, string.Empty);

        var existingReaction = await _repo.GetReactionAsync(messageId, userId);
        bool isRemoved = false;

        if (existingReaction != null)
        {
            if (existingReaction.Emoji == emoji)
            {
                await _repo.DeleteReactionAsync(existingReaction);
                isRemoved = true;
            }
            else
            {
                existingReaction.Emoji = emoji;
                existingReaction.Timestamp = DateTime.UtcNow;
            }
        }
        else
        {
            var newReaction = new MessageReaction
            {
                MessageId = messageId,
                UserId = userId,
                Emoji = emoji,
                Timestamp = DateTime.UtcNow
            };
            await _repo.AddReactionAsync(newReaction);
        }

        await _repo.SaveChangesAsync();
        var userName = user.NameAr ?? user.UserName ?? "Unknown";
        return (true, isRemoved, userName);
    }

    public async Task<(bool Success, DateTime ReadAt)> MarkAsReadAsync(int messageId, string userId)
    {
        var messageExists = await _repo.MessageExistsAsync(messageId);
        if (!messageExists) return (false, DateTime.MinValue);

        var alreadyRead = await _repo.HasReadStateAsync(messageId, userId);
        if (alreadyRead) return (false, DateTime.MinValue);

        var readAt = DateTime.UtcNow;
        var readState = new MessageReadState
        {
            MessageId = messageId,
            UserId = userId,
            ReadAt = readAt
        };

        await _repo.AddReadStateAsync(readState);
        await _repo.SaveChangesAsync();

        return (true, readAt);
    }
}
