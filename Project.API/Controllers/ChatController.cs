using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Project.API.Hubs;
using Project.BLL.Services;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project.API.Controllers;

public class EditMessageDto
{
    public string Content { get; set; } = string.Empty;
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(
        IChatService chatService, 
        IAuthService authService,
        IWebHostEnvironment env, 
        IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _authService = authService;
        _env = env;
        _hubContext = hubContext;
    }

    // 1. POST: api/chat/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadChatFile(
        IFormFile file,
        [FromForm] string? content = null,
        [FromForm] int? replyToMessageId = null)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("File size exceeds 10MB limit.");

        var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "chat");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/chat/{uniqueFileName}";
        var isImage = file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        var fileType = isImage ? "image" : "file";

        // Add message through BLL Service
        var responseMessage = await _chatService.AddMessageAsync(currentUserId, content ?? string.Empty, replyToMessageId, fileUrl, file.FileName, fileType);
        if (responseMessage == null)
            return BadRequest("Failed to add chat message.");

        // Broadcast immediately via SignalR Hub
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", responseMessage);

        return Ok(responseMessage);
    }

    // 2. GET: api/chat/history
    [HttpGet("history")]
    public async Task<IActionResult> GetChatHistory()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        var history = await _chatService.GetChatHistoryAsync();
        return Ok(history);
    }

    // 3. PUT: api/chat/messages/{id}
    [HttpPut("messages/{id:int}")]
    public async Task<IActionResult> EditMessage(int id, [FromBody] EditMessageDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        var result = await _chatService.EditMessageAsync(id, currentUserId, dto.Content);
        if (result == null)
        {
            return BadRequest("Failed to edit message or you are not the sender.");
        }

        // Broadcast update via SignalR
        await _hubContext.Clients.All.SendAsync("MessageEdited", result);

        return Ok(result);
    }

    // 4. DELETE: api/chat/messages/{id}
    [HttpDelete("messages/{id:int}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        var success = await _chatService.DeleteMessageAsync(id, currentUserId);
        if (!success)
        {
            return BadRequest("Failed to delete message or you are not the sender.");
        }

        // Broadcast deletion via SignalR
        await _hubContext.Clients.All.SendAsync("MessageDeleted", new { messageId = id });

        return Ok(new { success = true });
    }

    // 5. GET: api/chat/online-users
    [HttpGet("online-users")]
    public async Task<IActionResult> GetOnlineUsers()
    {
        var onlineUsers = await _chatService.GetOnlineUsersAsync(
            userId => ChatConnectionManager.OnlineUsers.ContainsKey(userId),
            userId => ChatConnectionManager.LastSeenTimes.TryGetValue(userId, out var seenTime) ? seenTime : (DateTime?)null,
            userId => ChatConnectionManager.LastActivityTimes.TryGetValue(userId, out var activityTime) ? activityTime : (DateTime?)null
        );

        return Ok(onlineUsers);
    }
}
