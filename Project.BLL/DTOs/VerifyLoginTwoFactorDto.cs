namespace Project.BLL.DTOs
{
    public class VerifyLoginTwoFactorDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
