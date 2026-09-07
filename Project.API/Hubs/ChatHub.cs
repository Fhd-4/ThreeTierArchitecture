using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Project.BLL.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.API.Hubs;

public static class ChatConnectionManager
{
    public static readonly ConcurrentDictionary<string, HashSet<string>> OnlineUsers
        = new ConcurrentDictionary<string, HashSet<string>>();

    public static readonly ConcurrentDictionary<string, DateTime> LastSeenTimes
        = new ConcurrentDictionary<string, DateTime>();

    public static readonly ConcurrentDictionary<string, DateTime> LastActivityTimes
        = new ConcurrentDictionary<string, DateTime>();
}

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IAuthService _authService; // To query user details easily

    public ChatHub(IChatService chatService, IAuthService authService)
    {
        _chatService = chatService;
        _authService = authService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            var connectionId = Context.ConnectionId;

            ChatConnectionManager.OnlineUsers.AddOrUpdate(
                userId,
                id => new HashSet<string> { connectionId },
                (id, set) =>
                {
                    lock (set)
                    {
                        set.Add(connectionId);
                    }
                    return set;
                }
            );

            ChatConnectionManager.LastActivityTimes[userId] = DateTime.UtcNow;
            ChatConnectionManager.LastSeenTimes.TryRemove(userId, out _);

            bool isNewlyOnline = false;
            if (ChatConnectionManager.OnlineUsers.TryGetValue(userId, out var set))
            {
                lock (set)
                {
                    isNewlyOnline = (set.Count == 1);
                }
            }

            if (isNewlyOnline)
            {
                await Clients.Others.SendAsync("UserStatusChanged", userId, true);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            var connectionId = Context.ConnectionId;
            bool isNewlyOffline = false;

            if (ChatConnectionManager.OnlineUsers.TryGetValue(userId, out var set))
            {
                lock (set)
                {
                    set.Remove(connectionId);
                    isNewlyOffline = (set.Count == 0);
                }

                if (isNewlyOffline)
                {
                    ChatConnectionManager.OnlineUsers.TryRemove(userId, out _);
                    ChatConnectionManager.LastSeenTimes[userId] = DateTime.UtcNow;
                }
            }

            if (isNewlyOffline)
            {
                await Clients.Others.SendAsync("UserStatusChanged", userId, false);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(
        string content,
        int? replyToMessageId = null,
        string? fileUrl = null,
        string? fileName = null,
        string? fileType = null)
    {
        var senderId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(senderId) || (string.IsNullOrWhiteSpace(content) && string.IsNullOrEmpty(fileUrl)))
        {
            return;
        }

        ChatConnectionManager.LastActivityTimes[senderId] = DateTime.UtcNow;

        var messageResult = await _chatService.AddMessageAsync(senderId, content, replyToMessageId, fileUrl, fileName, fileType);
        if (messageResult == null) return;

        await Clients.All.SendAsync("ReceiveMessage", messageResult);
    }

    public async Task StartTyping()
    {
        var senderId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(senderId)) return;

        ChatConnectionManager.LastActivityTimes[senderId] = DateTime.UtcNow;

        var profile = await _authService.GetUserProfileAsync(senderId);
        if (profile == null) return;

        var senderName = profile.NameAr ?? profile.UserName;
        await Clients.Others.SendAsync("UserTyping", senderName, true);
    }

    public async Task StopTyping()
    {
        var senderId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(senderId)) return;

        ChatConnectionManager.LastActivityTimes[senderId] = DateTime.UtcNow;

        var profile = await _authService.GetUserProfileAsync(senderId);
        if (profile == null) return;

        var senderName = profile.NameAr ?? profile.UserName;
        await Clients.Others.SendAsync("UserTyping", senderName, false);
    }

    public async Task SendReaction(int messageId, string emoji)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(emoji)) return;

        ChatConnectionManager.LastActivityTimes[userId] = DateTime.UtcNow;

        var (success, isRemoved, userName) = await _chatService.SendReactionAsync(messageId, userId, emoji);
        if (!success) return;

        await Clients.All.SendAsync("ReceiveReaction", new
        {
            messageId = messageId,
            userId = userId,
            userName = userName,
            emoji = emoji,
            isRemoved = isRemoved
        });
    }

    public async Task MarkMessageAsRead(int messageId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        var (success, readAt) = await _chatService.MarkAsReadAsync(messageId, userId);
        if (!success) return;

        await Clients.All.SendAsync("MessageRead", new
        {
            messageId = messageId,
            userId = userId,
            readAt = readAt
        });
    }

    public async Task EditMessage(int messageId, string newContent)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(newContent)) return;

        var result = await _chatService.EditMessageAsync(messageId, userId, newContent);
        if (result == null) return;

        await Clients.All.SendAsync("MessageEdited", result);
    }

    public async Task DeleteMessage(int messageId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        var success = await _chatService.DeleteMessageAsync(messageId, userId);
        if (!success) return;

        await Clients.All.SendAsync("MessageDeleted", new
        {
            messageId = messageId
        });
    }
}
