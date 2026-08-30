using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Project.BLL.DTOs;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepo;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        IEmailService emailService,
        IUserRepository userRepo)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _emailService = emailService;
        _userRepo = userRepo;
    }

    // Helper: Normalize Saudi phone
    private string? NormalizeAndValidateSaudiPhone(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var cleaned = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (cleaned.StartsWith("+966"))
        {
            var rest = cleaned.Substring(4);
            if (rest.Length == 9 && rest.All(char.IsDigit) && rest.StartsWith("5"))
            {
                return cleaned;
            }
        }
        else if (cleaned.StartsWith("966"))
        {
            var rest = cleaned.Substring(3);
            if (rest.Length == 9 && rest.All(char.IsDigit) && rest.StartsWith("5"))
            {
                return "+" + cleaned;
            }
        }
        else if (cleaned.StartsWith("05"))
        {
            var rest = cleaned.Substring(1);
            if (rest.Length == 9 && rest.All(char.IsDigit))
            {
                return "+966" + rest;
            }
        }
        else if (cleaned.StartsWith("5") && cleaned.Length == 9 && cleaned.All(char.IsDigit))
        {
            return "+966" + cleaned;
        }

        return null;
    }

    // Token generator helper
    private async Task<object> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtKey = _configuration["Jwt:Key"] ?? "defaultSecretKeyWithAtLeast32BytesLengthForSecurityPurpose";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var expiration = DateTime.UtcNow.AddHours(_configuration.GetValue<int?>("Jwt:ExpiryHours") ?? 600);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: authClaims,
            expires: expiration,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        );

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return new
        {
            id = user.Id,
            userId = user.Id,
            username = user.UserName,
            email = user.Email,
            phoneNumber = user.PhoneNumber,
            token = tokenValue,
            expiration,
            user = new
            {
                id = user.Id,
                username = user.UserName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                roles,
                isTwoFactorEnabled = user.IsTwoFactorEnabled
            }
        };
    }

    public async Task<(bool Succeeded, string Message, object? Result)> RegisterAsync(RegisterDto model)
    {
        var formattedPhone = NormalizeAndValidateSaudiPhone(model.PhoneNumber);
        if (formattedPhone == null)
        {
            return (false, "رقم الجوال غير صحيح! يجب أن يكون رقم جوال سعودي يبدأ بـ 5 أو 05 أو +966 ويحتوي على أرقام فقط.", null);
        }

        var phoneExists = await _userManager.Users.AnyAsync(u => u.PhoneNumber == formattedPhone);
        if (phoneExists)
            return (false, "رقم الجوال هذا مسجل مسبقاً!", null);

        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
            return (false, "اسم المستخدم هذا مسجل مسبقاً!", null);

        var user = new ApplicationUser
        {
            UserName = model.Username,
            PhoneNumber = formattedPhone,
            PhoneNumberConfirmed = true,
            Email = model.Email,
            NormalizedEmail = model.Email?.ToUpper() ?? string.Empty,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);
        }

        var defaultRole = "Member";
        var roleExists = await _roleManager.RoleExistsAsync(defaultRole);
        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole(defaultRole));
        }
        await _userManager.AddToRoleAsync(user, defaultRole);

        return (true, "تم تسجيل الحساب بنجاح!", new { message = "تم تسجيل الحساب بنجاح!" });
    }

    public async Task<(bool Succeeded, string Message, bool RequiresTwoFactor, string? UserId, object? TokenResult)> LoginAsync(LoginDto model)
    {
        var formattedPhone = NormalizeAndValidateSaudiPhone(model.PhoneNumber);
        if (formattedPhone == null)
        {
            return (false, "رقم الجوال غير صحيح! يجب أن يكون رقم جوال سعودي يبدأ بـ 5 أو 05 أو +966.", false, null, null);
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == formattedPhone);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return (false, "رقم الجوال أو الرقم السري غير صحيح!", false, null, null);
        }

        if (user.IsTwoFactorEnabled)
        {
            var randomCode = new Random().Next(100000, 999999).ToString();
            user.TwoFactorCode = randomCode;
            user.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    var subject = "رمز التحقق الثنائي لتسجيل الدخول - ProSync";
                    var body = $@"
                    <div style='font-family: Arial, sans-serif; direction: rtl; text-align: right; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                        <h2 style='color: #007bff;'>مرحباً {user.UserName}،</h2>
                        <p style='font-size: 1.1rem; color: #4a5568;'>تلقينا محاولة تسجيل دخول إلى حسابك على منصة ProSync.</p>
                        <p style='font-size: 1.1rem; color: #4a5568;'>رمز التحقق (OTP) الخاص بك هو:</p>
                        <div style='background: #f7fafc; padding: 15px; border-radius: 8px; text-align: center; font-size: 1.8rem; font-weight: bold; letter-spacing: 4px; color: #1a2b4c; margin: 20px 0;'>
                            {randomCode}
                        </div>
                        <p style='font-size: 0.95rem; color: #718096;'>هذا الرمز صالح لمدة 5 دقائق فقط. إذا لم تقم بهذا الطلب، يرجى تجاهل هذه الرسالة.</p>
                    </div>";

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[2FA EMAIL ERROR]: {ex.Message}");
                }
            }

            return (true, "تم إرسال رمز التحقق إلى بريدك الإلكتروني بنجاح!", true, user.Id, null);
        }

        var token = await GenerateJwtTokenAsync(user);
        return (true, "تم تسجيل الدخول بنجاح", false, user.Id, token);
    }

    public async Task<(bool Succeeded, string Message, object? TokenResult)> VerifyLogin2FaAsync(VerifyLoginTwoFactorDto model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return (false, "بيانات غير صالحة!", null);
        }

        if (string.IsNullOrEmpty(user.TwoFactorCode) ||
            user.TwoFactorCode != model.Code ||
            user.TwoFactorCodeExpiry == null ||
            user.TwoFactorCodeExpiry < DateTime.UtcNow)
        {
            return (false, "رمز التحقق غير صحيح أو انتهت صلاحيته!", null);
        }

        user.TwoFactorCode = null;
        user.TwoFactorCodeExpiry = null;
        await _userManager.UpdateAsync(user);

        var token = await GenerateJwtTokenAsync(user);
        return (true, "تم التحقق وتسجيل الدخول بنجاح", token);
    }

    public async Task<(bool Succeeded, string Message)> ToggleTwoFactorAsync(ToggleTwoFactorDto model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return (false, "المستخدم غير موجود!");
        }

        user.IsTwoFactorEnabled = model.Enable;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, model.Enable ? "تم تفعيل التحقق بخطوتين بنجاح!" : "تم تعطيل التحقق بخطوتين بنجاح!");
    }

    public async Task<(bool Succeeded, string Message, object? Result)> GetTwoFactorStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "المستخدم غير موجود!", null);
        }

        return (true, "تم جلب الحالة بنجاح", new
        {
            userId = user.Id,
            isTwoFactorEnabled = user.IsTwoFactorEnabled
        });
    }

    public async Task<IEnumerable<UserListItemDto>> GetAllUsersAsync()
    {
        var usersList = await _userManager.Users.ToListAsync();
        var result = new List<UserListItemDto>();

        foreach (var u in usersList)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var roleName = roles.FirstOrDefault() ?? "Member";
            
            // Check online state mock or integration
            var isOnline = false;

            result.Add(new UserListItemDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Role = roleName,
                NameAr = u.NameAr,
                NameEn = u.NameEn,
                TitleAr = u.TitleAr,
                TitleEn = u.TitleEn,
                CreatedDate = u.CreatedDate,
                IsActive = u.IsActive,
                IsOnline = isOnline
            });
        }

        return result;
    }

    public async Task<(bool Succeeded, string Message)> ChangePasswordAsync(ChangePasswordDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return (false, "البريد الإلكتروني مطلوب!");
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return (false, "المستخدم غير موجود!");
        }

        IdentityResult result;
        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        }
        else
        {
            result = await _userManager.AddPasswordAsync(user, model.NewPassword);
        }

        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, "تم تغيير الرقم السري بنجاح!");
    }

    public async Task<(bool Succeeded, string Message)> ForgotPasswordAsync(ForgotPasswordDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return (false, "البريد الإلكتروني مطلوب!");
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return (false, "المستخدم غير موجود!");
        }

        var token = await _userManager.GenerateUserTokenAsync(user, "Email", "ResetPassword");

        try
        {
            var subject = "رمز التحقق لإعادة تعيين كلمة المرور - ProSync";
            var body = $@"
            <div style='font-family: Arial, sans-serif; direction: rtl; text-align: right; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                <h2 style='color: #007bff;'>مرحباً {user.UserName}،</h2>
                <p style='font-size: 1.1rem; color: #4a5568;'>لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بك على منصة ProSync.</p>
                <p style='font-size: 1.1rem; color: #4a5568;'>رمز التحقق (OTP) الخاص بك هو:</p>
                <div style='background: #f7fafc; padding: 15px; border-radius: 8px; text-align: center; font-size: 1.8rem; font-weight: bold; letter-spacing: 4px; color: #1a2b4c; margin: 20px 0;'>
                    {token}
                </div>
                <p style='font-size: 0.95rem; color: #718096;'>هذا الرمز صالحة لمدة محدودة. إذا لم تقم بطلب إعادة التعيين بنفسك، يرجى تجاهل هذا البريد الإلكتروني.</p>
                <hr style='border: none; border-top: 1px solid #edf2f7; margin: 20px 0;' />
                <p style='font-size: 0.85rem; color: #a0aec0; text-align: center;'>فريق حماية ProSync Security</p>
            </div>";

            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL SEND FAILURE]: {ex.Message}");
        }

        return (true, "تم توليد رمز إعادة تعيين كلمة المرور بنجاح وإرساله للبريد الإلكتروني!");
    }

    public async Task<(bool Succeeded, string Message)> VerifyOtpAsync(VerifyOtpDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return (false, "المستخدم غير موجود!");
        }

        var isValid = await _userManager.VerifyUserTokenAsync(user, "Email", "ResetPassword", model.Token);
        if (!isValid)
        {
            return (false, "رمز التحقق غير صحيح!");
        }

        return (true, "رمز التحقق صحيح!");
    }

    public async Task<(bool Succeeded, string Message, IEnumerable<object>? Errors)> ResetPasswordAsync(ResetPasswordDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return (false, "البريد الإلكتروني مطلوب!", null);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return (false, "المستخدم غير موجود!", null);
        }

        var isValid = await _userManager.VerifyUserTokenAsync(user, "Email", "ResetPassword", model.Token);
        if (!isValid)
        {
            return (false, "رمز إعادة التعيين غير صحيح أو انتهت صلاحيته!", new[] { new { code = "InvalidToken", description = "رمز إعادة التعيين غير صحيح أو انتهت صلاحيته!" } });
        }

        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                return (false, "فشل حذف كلمة المرور القديمة", removeResult.Errors);
            }
        }

        var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
        if (!result.Succeeded)
        {
            return (false, "فشل إضافة كلمة المرور الجديدة", result.Errors);
        }

        return (true, "تم إعادة تعيين كلمة المرور الجديدة بنجاح!", null);
    }

    public async Task<(bool Succeeded, string Message)> CreateRoleAsync(CreateRoleDto model)
    {
        var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
        if (roleExists)
        {
            return (false, "Role already exists");
        }

        var role = new IdentityRole(model.RoleName);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, "Role created successfully");
    }

    public async Task<IEnumerable<object>> GetAllRolesAsync()
    {
        var roles = await _roleManager.Roles
            .Select(r => new
            {
                r.Id,
                r.Name
            })
            .ToListAsync();

        return roles;
    }

    public async Task<(bool Succeeded, string Message)> CreateSuperAdminAsync(CreateSuperAdminDto model)
    {
        var formattedPhone = NormalizeAndValidateSaudiPhone(model.PhoneNumber);
        if (formattedPhone == null)
        {
            return (false, "رقم الجوال غير صحيح!");
        }

        var roleExists = await _roleManager.RoleExistsAsync("SuperAdmin");
        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        }

        var userExists = await _userManager.Users.AnyAsync(u => u.PhoneNumber == formattedPhone);
        if (userExists)
        {
            return (false, "User with this phone number already exists");
        }

        var user = new ApplicationUser
        {
            UserName = model.Username,
            PhoneNumber = formattedPhone,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, "SuperAdmin");

        return (true, "SuperAdmin created successfully");
    }

    public async Task<(bool Succeeded, string Message)> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "المستخدم غير موجود!");
        }

        var (hasPortfolios, hasPrograms, hasProjects, hasTasks, hasChangeRequests) = await _userRepo.CheckUserDependenciesAsync(userId);
        
        if (hasPortfolios)
            return (false, "لا يمكن حذف المستخدم لأنه مالك لمحفظة واحدة أو أكثر. يرجى نقل ملكية المحفظة أولاً.");
        
        if (hasPrograms)
            return (false, "لا يمكن حذف المستخدم لأنه مدير لبرنامج واحد أو أكثر. يرجى تعيين مدير آخر للبرنامج أولاً.");
        
        if (hasProjects)
            return (false, "لا يمكن حذف المستخدم لأنه مدير لمشروع واحد أو أكثر. يرجى تعيين مدير آخر للمشروع أولاً.");
        
        if (hasTasks)
            return (false, "لا يمكن حذف المستخدم لأنه معين على مهمة واحدة أو أكثر. يرجى إزالة التعيين أو نقله لمستخدم آخر أولاً.");
        
        if (hasChangeRequests)
            return (false, "لا يمكن حذف المستخدم لأنه قام بطلب طلبات تغيير (Change Requests) نشطة. يرجى مراجعتها أولاً.");

        // Clean dependencies
        await _userRepo.CleanUserDependenciesAsync(userId);

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, "تم حذف المستخدم بنجاح!");
    }

    public async Task<(bool Succeeded, string Message, string? UserId)> CreateUserAsync(CreateUserDto model)
    {
        var formattedPhone = NormalizeAndValidateSaudiPhone(model.PhoneNumber);
        if (formattedPhone == null)
        {
            return (false, "رقم الجوال غير صحيح! يجب أن يكون رقم جوال سعودي يبدأ بـ 5 أو 05 أو +966.", null);
        }

        var phoneExists = await _userManager.Users.AnyAsync(u => u.PhoneNumber == formattedPhone);
        if (phoneExists)
            return (false, "رقم الجوال هذا مسجل مسبقاً!", null);

        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
            return (false, "اسم المستخدم هذا مسجل مسبقاً!", null);

        var user = new ApplicationUser
        {
            UserName = model.Username,
            Email = model.Email,
            PhoneNumber = formattedPhone,
            PhoneNumberConfirmed = true,
            NameAr = model.NameAr,
            NameEn = model.NameEn,
            TitleAr = model.TitleAr,
            TitleEn = model.TitleEn,
            IsActive = model.IsActive,
            CreatedDate = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);
        }

        var roleExists = await _roleManager.RoleExistsAsync(model.Role);
        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        }
        await _userManager.AddToRoleAsync(user, model.Role);

        // Associate user with entities
        await _userRepo.AssociateUserWithEntitiesAsync(user.Id, model.PortfolioId, model.ProgramId, model.ProjectId);

        return (true, "تم إنشاء المستخدم بنجاح!", user.Id);
    }

    public async Task<(bool Succeeded, string Message)> UpdateUserAsync(string userId, UpdateUserDto model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "المستخدم غير موجود!");
        }

        var formattedPhone = NormalizeAndValidateSaudiPhone(model.PhoneNumber);
        if (formattedPhone == null)
        {
            return (false, "رقم الجوال غير صحيح!");
        }

        var phoneExists = await _userManager.Users.AnyAsync(u => u.PhoneNumber == formattedPhone && u.Id != userId);
        if (phoneExists)
            return (false, "رقم الجوال هذا مسجل مسبقاً لمستخدم آخر!");

        if (!string.IsNullOrWhiteSpace(model.Username) && user.UserName != model.Username)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return (false, "اسم المستخدم هذا مسجل مسبقاً لمستخدم آخر!");
            }
            user.UserName = model.Username;
            user.NormalizedUserName = model.Username.ToUpper();
        }

        user.Email = model.Email;
        user.NormalizedEmail = model.Email.ToUpper();
        user.PhoneNumber = formattedPhone;
        user.PhoneNumberConfirmed = true;
        user.NameAr = model.NameAr;
        user.NameEn = model.NameEn;
        user.TitleAr = model.TitleAr;
        user.TitleEn = model.TitleEn;
        user.IsActive = model.IsActive;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
            if (!passResult.Succeeded)
            {
                return (false, string.Join(", ", passResult.Errors.Select(e => e.Description)));
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var roleExists = await _roleManager.RoleExistsAsync(model.Role);
        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        }
        await _userManager.AddToRoleAsync(user, model.Role);

        // Associate user updates
        await _userRepo.AssociateUserWithEntitiesAsync(user.Id, model.PortfolioId, model.ProgramId, model.ProjectId);

        return (true, "تم تحديث بيانات المستخدم بنجاح!");
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string id)
    {
        var u = await _userManager.FindByIdAsync(id);
        if (u == null) return null;

        var roles = await _userManager.GetRolesAsync(u);
        var roleName = roles.FirstOrDefault() ?? "Member";

        var dto = new UserProfileDto
        {
            Id = u.Id,
            UserName = u.UserName ?? string.Empty,
            Email = u.Email ?? string.Empty,
            PhoneNumber = u.PhoneNumber ?? string.Empty,
            Role = roleName,
            NameAr = u.NameAr,
            NameEn = u.NameEn,
            TitleAr = u.TitleAr,
            TitleEn = u.TitleEn,
            CreatedDate = u.CreatedDate,
            IsActive = u.IsActive
        };

        var portfolios = await _userRepo.GetPortfoliosByOwnerIdAsync(id);
        dto.Portfolios = portfolios.Select(p => new UserPortfolioDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Category ?? "Execution",
            ProgramsCount = p.Programs?.Count ?? 0,
            ProjectsCount = p.Projects?.Count ?? 0,
            Progress = 0,
            Status = p.Status == 1 ? "Active" : p.Status == 2 ? "Completed" : p.Status == 3 ? "Pending" : "Rejected"
        }).ToList();

        var programs = await _userRepo.GetProgramsByManagerIdAsync(id);
        dto.Programs = programs.Select(pr => new UserProgramDto
        {
            Id = pr.Id,
            Name = pr.Name,
            Category = "Execution",
            ProjectsCount = pr.Projects?.Count ?? 0,
            Progress = pr.ProgressPercentage,
            Status = pr.Status == 1 ? "Active" : pr.Status == 2 ? "Completed" : pr.Status == 3 ? "Pending" : "Rejected"
        }).ToList();

        var projects = await _userRepo.GetProjectsByManagerIdAsync(id);
        dto.Projects = projects.Select(proj => new UserProjectDto
        {
            Id = proj.Id,
            Name = proj.Name,
            Category = "Execution",
            TasksCount = proj.Tasks?.Count ?? 0,
            Progress = 0,
            Status = proj.Status == 1 ? "Active" : proj.Status == 2 ? "Completed" : proj.Status == 3 ? "Pending" : "Rejected"
        }).ToList();

        return dto;
    }
}
