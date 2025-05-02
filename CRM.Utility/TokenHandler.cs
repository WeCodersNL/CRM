using CRM.Utility.IUtility;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CRM.Utility
{
    public class TokenHandler(IOptions<TokenConfiguration> tokenConfig) : ITokenHandler
    {
        private readonly TokenConfiguration _tokenConfig = tokenConfig.Value;

        public string GenerateJwtToken(List<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenConfig.SecretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenValidity = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_tokenConfig.TokenExpiry));

            var token = new JwtSecurityToken(
                        issuer: _tokenConfig.Issuer,
                        audience: _tokenConfig.Audience,
                        claims: claims,
                        expires: tokenValidity,
                        signingCredentials: signingCredentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _tokenConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = _tokenConfig.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenConfig.SecretKey)),
                ValidateLifetime = false // <— allows expired token
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public int GetRefreshTokenExpiryDays() => _tokenConfig.RefreshTokenExpiryDays;

        public int GetMaxRefreshTokenAttempts() => _tokenConfig.MaxRefreshTokenAttempts;
    }
}
