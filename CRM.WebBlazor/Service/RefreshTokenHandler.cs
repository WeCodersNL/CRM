using CRM.Model.ApplicationModels;

namespace CRM.WebBlazor.Service
{
    public class RefreshTokenHandler(IHttpClientFactory httpClientFactory, TokenStore tokenStore)
    {
        private readonly HttpClient httpClient = httpClientFactory.CreateClient("DefaultClient");
        public async Task<bool> TryRefreshTokenAsync()
        {
            var refreshToken = tokenStore.RefreshToken;
            var accessToken = tokenStore.AccessToken;

            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var refreshResponse = await httpClient.PostAsJsonAsync("Identity/Authentication/refresh-token", new AuthenticationTokens
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });

            if (refreshResponse.IsSuccessStatusCode)
            {
                var result = await refreshResponse.Content.ReadFromJsonAsync<ResponseModel<AuthenticationTokens>>();
                if (result?.IsSuccess == true && result.Data != null)
                {
                    tokenStore.AccessToken = result.Data.AccessToken;
                    tokenStore.RefreshToken = result.Data.RefreshToken;
                    tokenStore.IsAuthenticated = true;
                    return true;
                }
            }

            return false;
        }
    }
}
