using System.ComponentModel.DataAnnotations;

namespace Project.BLL.DTOs
{
    public class CreateRoleDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}

