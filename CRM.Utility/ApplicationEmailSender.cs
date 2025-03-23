using CRM.Utility.IUtility;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace CRM.Utility
{
    public class ApplicationEmailSender(IOptions<EmailConfiguration> emailConfiguration) : IApplicationEmailSender
    {
        private readonly EmailConfiguration _emailConfiguration = emailConfiguration.Value;

        public Task SendEmailAsync(MailMessage message)
        {
            message.From = new MailAddress(_emailConfiguration.FromEmail, _emailConfiguration.FromName);
            SmtpClient smtpClient = new(_emailConfiguration.SmtpServer, _emailConfiguration.Port)
            {
                Credentials = new System.Net.NetworkCredential(_emailConfiguration.UserName, _emailConfiguration.Password),
                EnableSsl = true
            };
            return smtpClient.SendMailAsync(message);
        }
    }
}
