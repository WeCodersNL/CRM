using CRM.Model.ApplicationModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;

namespace CRM.WebBlazor.Service
{
    public class AuthenticatedHttpClientService(
        IHttpClientFactory httpClientFactory
        , RefreshTokenHandler refreshTokenHandler
        , IJSRuntime jsRuntime
        , TokenStore tokenStore
        , IErrorHandlingService errorHandlingService
        , NavigationManager navManager
        )
    {
        private readonly HttpClient httpClient = httpClientFactory.CreateClient("SecureClient");

        public async Task<ResponseModel<T>> GetAsync<T>(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync<ResponseModel<T>>();
            return result is null || !result.IsSuccess
                ? errorHandlingService.HandleErrorResponse(result!)
                : result;
        }

        public Task<ResponseModel<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data)
            => SendJsonAsync<TRequest, TResponse>(HttpMethod.Post, url, data);

        public Task<ResponseModel<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest data)
            => SendJsonAsync<TRequest, TResponse>(HttpMethod.Put, url, data);

        public async Task<ResponseModel<bool>> DeleteAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            var response = await SendAsync(request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseModel<bool>>();
            return result ?? new ResponseModel<bool> { IsSuccess = true };
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            if (!tokenStore.IsAuthenticated)
                await tokenStore.LoadFromStorageAsync(jsRuntime);

            try
            {
                if (tokenStore.IsAccessTokenExpired())
                {
                    var refreshed = await refreshTokenHandler.TryRefreshTokenAsync();
                    if (!refreshed)
                    {
                        await tokenStore.ClearStorageAsync(jsRuntime);
                        navManager.NavigateTo("/identity/login", forceLoad: true);
                    }
                    await tokenStore.SaveToStorageAsync(jsRuntime);
                }

                await AddAuthorizationHeaderAsync(request);
                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    // Try to refresh the token
                    var success = await refreshTokenHandler.TryRefreshTokenAsync();
                    if (success)
                    {
                        // Retry the original request with the new token
                        var newRequest = CloneRequest(request);
                        await AddAuthorizationHeaderAsync(newRequest);
                        await tokenStore.SaveToStorageAsync(jsRuntime);
                        response = await httpClient.SendAsync(newRequest);
                    }
                    else
                    {
                        await tokenStore.ClearStorageAsync(jsRuntime);
                        navManager.NavigateTo("/identity/login", forceLoad: true);
                    }
                }
                return response;
            }
            catch (HttpRequestException ex)
            {
                await tokenStore.ClearStorageAsync(jsRuntime);
                navManager.NavigateTo("/identity/login", forceLoad: true);
                throw new Exception("Network error occurred while sending the request.", ex);
            }
        }

        private async Task<ResponseModel<TResponse>> SendJsonAsync<TRequest, TResponse>(HttpMethod method, string url, TRequest data)
        {
            var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(data)
            };
            var response = await SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync<ResponseModel<TResponse>>();
            return result is null || !result.IsSuccess
                ? errorHandlingService.HandleErrorResponse(result!)
                : result;
        }

        private Task AddAuthorizationHeaderAsync(HttpRequestMessage request)
        {
            var token = tokenStore.AccessToken;
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return Task.CompletedTask;
        }

        private HttpRequestMessage CloneRequest(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content,
                Version = request.Version,
                VersionPolicy = request.VersionPolicy
            };

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
