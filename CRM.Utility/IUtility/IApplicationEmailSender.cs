using System.Net.Mail;

namespace CRM.Utility.IUtility
{
    public interface IApplicationEmailSender
    {
        Task SendEmailAsync(MailMessage message);
    }
}
