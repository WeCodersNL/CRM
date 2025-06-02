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
        )
    {
        private readonly HttpClient httpClient = httpClientFactory.CreateClient("SecureClient");

        public async Task<ResponseModel<T>> GetAsync<T>(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await SendAsync(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
                errorHandlingService.RedirectToErrorPage("An internal server error occurred");

            var result = await response.Content.ReadFromJsonAsync<ResponseModel<T>>();
            return errorHandlingService.EnsureSuccessOrHandle(result);
        }

        public Task<ResponseModel<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data)
            => SendJsonAsync<TRequest, TResponse>(HttpMethod.Post, url, data);

        public Task<ResponseModel<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest data)
            => SendJsonAsync<TRequest, TResponse>(HttpMethod.Put, url, data);

        public async Task<ResponseModel<bool>> DeleteAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            var response = await SendAsync(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
                errorHandlingService.RedirectToErrorPage("An internal server error occurred during deletion");

            var result = await response.Content.ReadFromJsonAsync<ResponseModel<bool>>();
            return errorHandlingService.EnsureSuccessOrHandle(result);
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            if (!tokenStore.IsAuthenticated)
                await tokenStore.LoadFromStorageAsync(jsRuntime);

            try
            {
                //throw new HttpRequestException("Simulated network error for testing.");
                if (tokenStore.IsAccessTokenExpired())
                {
                    var refreshed = await refreshTokenHandler.TryRefreshTokenAsync();
                    if (!refreshed)
                    {
                        await tokenStore.ClearStorageAsync(jsRuntime);
                        errorHandlingService.HandleUnauthorized();
                    }
                    else
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
                        errorHandlingService.HandleUnauthorized();
                    }
                }
                return response;
            }
            catch (HttpRequestException ex)
            {
                await tokenStore.ClearStorageAsync(jsRuntime);
                errorHandlingService.RedirectToErrorPage(ex.Message);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
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
            return errorHandlingService.EnsureSuccessOrHandle(result);
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
