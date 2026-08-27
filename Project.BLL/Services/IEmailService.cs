using System.Threading.Tasks;

namespace Project.BLL.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string body);
    }
}

