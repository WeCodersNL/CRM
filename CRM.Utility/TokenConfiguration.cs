namespace CRM.Utility
{
    public class TokenConfiguration
    {
        public required string Subject { get; set; }
        public required string SecretKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string TokenExpiry { get; set; }
    }
}
