using System.ComponentModel.DataAnnotations;

namespace Project.BLL.DTOs
{
    public class CreateSuperAdminDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}

