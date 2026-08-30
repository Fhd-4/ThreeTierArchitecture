using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.BLL.DTOs;
using Project.BLL.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // 1. Register: api/Auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var (succeeded, message, result) = await _authService.RegisterAsync(model);
        if (!succeeded)
        {
            return BadRequest(message);
        }
        return Ok(result);
    }

    // 2. Login: api/Auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var (succeeded, message, requiresTwoFactor, userId, tokenResult) = await _authService.LoginAsync(model);
        if (!succeeded)
        {
            return Unauthorized(message);
        }

        if (requiresTwoFactor)
        {
            return Ok(new
            {
                requiresTwoFactor = true,
                userId = userId,
                message = message
            });
        }

        // Serilog login logging
        var timeString = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        Log.Information("User LOGIN: '{Username}' logged in at {Time}", model.PhoneNumber, timeString);

        return Ok(tokenResult);
    }

    // 3. Verify Login 2FA: api/Auth/verify-login-2fa
    [HttpPost("verify-login-2fa")]
    public async Task<IActionResult> VerifyLogin2Fa([FromBody] VerifyLoginTwoFactorDto model)
    {
        var (succeeded, message, tokenResult) = await _authService.VerifyLogin2FaAsync(model);
        if (!succeeded)
        {
            return BadRequest(message);
        }

        // Serilog login logging
        var timeString = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        Log.Information("User LOGIN 2FA: '{UserId}' logged in at {Time}", model.UserId, timeString);

        return Ok(tokenResult);
    }

    // 4. Toggle 2FA status: api/Auth/toggle-2fa
    [HttpPost("toggle-2fa")]
    public async Task<IActionResult> ToggleTwoFactor([FromBody] ToggleTwoFactorDto model)
    {
        var (succeeded, message) = await _authService.ToggleTwoFactorAsync(model);
        if (!succeeded)
        {
            return BadRequest(message);
        }
        return Ok(new { message });
    }

    // 5. GET: api/Auth/2fa-status/{userId}
    [HttpGet("2fa-status/{userId}")]
    public async Task<IActionResult> GetTwoFactorStatus(string userId)
    {
        var (succeeded, message, result) = await _authService.GetTwoFactorStatusAsync(userId);
        if (!succeeded)
        {
            return NotFound(message);
        }
        return Ok(result);
    }

    // 6. Get All Users: api/Auth/all-users
    [HttpGet("all-users")]
    public async Task<ActionResult<IEnumerable<UserListItemDto>>> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    // 7. Change Password: api/Auth/change-password
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
    {
        var (succeeded, message) = await _authService.ChangePasswordAsync(model);
        if (!succeeded)
        {
            return BadRequest(message);
        }
        return Ok(new { message });
    }

    // 8. Forgot Password: api/Auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var (succeeded, message) = await _authService.ForgotPasswordAsync(model);
        if (!succeeded)
        {
            return BadRequest(message);
        }
        return Ok(new { message });
    }

    // 9. Verify OTP: api/Auth/verify-otp
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (succeeded, message) = await _authService.VerifyOtpAsync(model);
        if (!succeeded)
        {
            return BadRequest(message);
        }
        return Ok(new { success = true, message });
    }

    // 10. Reset Password: api/Auth/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var (succeeded, message, errors) = await _authService.ResetPasswordAsync(model);
        if (!succeeded)
        {
            return BadRequest(errors ?? new object[] { new { code = "ResetFailed", description = message } });
        }
        return Ok(new { message });
    }

    // 11. Create Role: api/Auth/create-role
    [HttpPost("create-role")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto model)
    {
        var (succeeded, message) = await _authService.CreateRoleAsync(model);
        if (!succeeded)
        {
            return BadRequest(message);
        }
        return Ok(message);
    }

    // 12. Get All Roles: api/Auth/roles
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _authService.GetAllRolesAsync();
        return Ok(roles);
    }

    // 13. Create Super Admin: api/Auth/create-superadmin
    [HttpPost("create-superadmin")]
    public async Task<IActionResult> CreateSuperAdmin([FromBody] CreateSuperAdminDto model)
    {
        var (succeeded, message) = await _authService.CreateSuperAdminAsync(model);
        if (!succeeded)
        {
            return BadRequest(message);
        }
        return Ok(message);
    }

    // 14. Delete User: api/Auth/delete-user/{userId}
    [HttpDelete("delete-user/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var (succeeded, message) = await _authService.DeleteUserAsync(userId);
        if (!succeeded)
        {
            return BadRequest(message);
        }
        return Ok(new { message });
    }

    // 15. Create User: api/Auth/create-user
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (succeeded, message, newUserId) = await _authService.CreateUserAsync(dto);
        if (!succeeded)
        {
            return BadRequest(message);
        }

        return Ok(new { message, userId = newUserId });
    }

    // 16. Update User: api/Auth/update-user/{userId}
    [HttpPut("update-user/{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (succeeded, message) = await _authService.UpdateUserAsync(userId, dto);
        if (!succeeded)
        {
            return BadRequest(message);
        }

        return Ok(new { message });
    }

    // 17. User Profile Details: api/Auth/user-profile/{id}
    [HttpGet("user-profile/{id}")]
    public async Task<IActionResult> GetUserProfile(string id)
    {
        var profile = await _authService.GetUserProfileAsync(id);
        if (profile == null)
        {
            return NotFound("المستخدم غير موجود!");
        }
        return Ok(profile);
    }

    // 18. Logout (Activity logs compatibility)
    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest("اسم المستخدم مطلوب");
        }

        var timeString = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        Log.Information("User LOGOUT: '{Username}' logged out at {Time}", request.Username, timeString);

        return Ok(new { message = $"تم تسجيل خروج المستخدم '{request.Username}' بنجاح." });
    }
}
