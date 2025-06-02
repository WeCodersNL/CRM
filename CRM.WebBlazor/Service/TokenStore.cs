using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;

namespace CRM.WebBlazor.Service
{
    public class TokenStore
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set
            {
                if (_isAuthenticated != value)
                {
                    _isAuthenticated = value;
                    OnAuthenticationStateChanged?.Invoke(_isAuthenticated);
                }
            }
        }

        public event Action<bool>? OnAuthenticationStateChanged;

        public bool IsInitialized => !string.IsNullOrEmpty(AccessToken) || !string.IsNullOrEmpty(RefreshToken);

        public bool IsAccessTokenExpired()
        {
            if (string.IsNullOrEmpty(AccessToken))
                return true;

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(AccessToken);
            return jwt.ValidTo < DateTime.UtcNow;
        }

        public async Task SaveToStorageAsync(IJSRuntime jsRuntime)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", "access-token", AccessToken);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", "refresh-token", RefreshToken);
        }

        public async Task LoadFromStorageAsync(IJSRuntime jsRuntime)
        {
            AccessToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "access-token");
            RefreshToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "refresh-token");
            IsAuthenticated = !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(RefreshToken);
        }

        public async Task ClearStorageAsync(IJSRuntime jsRuntime)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", "access-token");
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refresh-token");

            AccessToken = null;
            RefreshToken = null;
            IsAuthenticated = false;
        }

        public string? GetEmailFromAccessToken()
        {
            if (string.IsNullOrEmpty(AccessToken))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(AccessToken);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
        }
    }
}
