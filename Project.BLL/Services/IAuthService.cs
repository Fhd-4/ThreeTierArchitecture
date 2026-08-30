using Project.BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public interface IAuthService
{
    Task<(bool Succeeded, string Message, object? Result)> RegisterAsync(RegisterDto model);
    Task<(bool Succeeded, string Message, bool RequiresTwoFactor, string? UserId, object? TokenResult)> LoginAsync(LoginDto model);
    Task<(bool Succeeded, string Message, object? TokenResult)> VerifyLogin2FaAsync(VerifyLoginTwoFactorDto model);
    Task<(bool Succeeded, string Message)> ToggleTwoFactorAsync(ToggleTwoFactorDto model);
    Task<(bool Succeeded, string Message, object? Result)> GetTwoFactorStatusAsync(string userId);
    Task<IEnumerable<UserListItemDto>> GetAllUsersAsync();
    Task<(bool Succeeded, string Message)> ChangePasswordAsync(ChangePasswordDto model);
    Task<(bool Succeeded, string Message)> ForgotPasswordAsync(ForgotPasswordDto model);
    Task<(bool Succeeded, string Message)> VerifyOtpAsync(VerifyOtpDto model);
    Task<(bool Succeeded, string Message, IEnumerable<object>? Errors)> ResetPasswordAsync(ResetPasswordDto model);
    Task<(bool Succeeded, string Message)> CreateRoleAsync(CreateRoleDto model);
    Task<IEnumerable<object>> GetAllRolesAsync();
    Task<(bool Succeeded, string Message)> CreateSuperAdminAsync(CreateSuperAdminDto model);
    Task<(bool Succeeded, string Message)> DeleteUserAsync(string userId);
    Task<(bool Succeeded, string Message, string? UserId)> CreateUserAsync(CreateUserDto model);
    Task<(bool Succeeded, string Message)> UpdateUserAsync(string userId, UpdateUserDto model);
    Task<UserProfileDto?> GetUserProfileAsync(string id);
}
