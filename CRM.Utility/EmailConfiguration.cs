namespace CRM.Utility
{
    public class EmailConfiguration
    {
        public required string FromName { get; set; }
        public required string FromEmail { get; set; }
        public required string SmtpServer { get; set; }
        public int Port { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
