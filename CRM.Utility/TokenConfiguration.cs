namespace CRM.Utility
{
    public class TokenConfiguration
    {
        public required string Subject { get; set; }
        public required string SecretKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string TokenExpiry { get; set; }
        public required int RefreshTokenExpiryDays { get; set; }
        public required int MaxRefreshTokenAttempts { get; set; }
    }
}
