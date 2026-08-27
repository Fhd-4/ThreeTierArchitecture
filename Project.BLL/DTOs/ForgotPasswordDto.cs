using System.ComponentModel.DataAnnotations;

namespace Project.BLL.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

