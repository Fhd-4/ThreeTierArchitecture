namespace Project.API.Models;

public class UserActivityDto
{
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Login or Logout
    public string Timestamp { get; set; } = string.Empty;
}

