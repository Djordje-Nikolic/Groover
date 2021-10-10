using Groover.AvaloniaUI.Models.Interfaces;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services
{
    public enum Controller
    {
        User,
        Group,
        GroupChat
    }

    public class ApiService : IApiService
    {
        public IApiConfiguration ApiConfig { get; private set; }
        private HttpClient _httpClient;
        private HttpClientHandler _httpClientHandler;
        private CookieContainer _cookieContainer { get { return _httpClientHandler.CookieContainer; } }

        public ApiService(IApiConfiguration apiConfiguration)
        {
            ApiConfig = apiConfiguration;

            InitializeHttpClient();
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, 
            Controller controller, 
            string endpointMethod,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead)
        {
            message.RequestUri = MakeUri(controller, endpointMethod);
            
            var response = await _httpClient.SendAsync(message, completionOption);

            return response;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead)
        {
            var response = await _httpClient.SendAsync(message, completionOption);

            return response;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, 
            Controller controller, 
            string endpointMethod, 
            string queryParams, 
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead)
        {
            message.RequestUri = MakeUri(controller, endpointMethod, queryParams);

            var response = await _httpClient.SendAsync(message, completionOption);

            return response;
        }

        public void SetAccessToken(string token, string type = "Bearer")
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(type, token);
        }

        public string? GetAccessToken()
        {
            return _httpClient.DefaultRequestHeaders.Authorization?.Parameter;
        }

        public async Task RefreshTokenAsync()
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, string.Empty);

            var response = await SendAsync(message, Controller.User, "RefreshToken");
            var responseContent = await response.Content.ReadAsStringAsync();

            TokenResponse parsedResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                parsedResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                if (!string.IsNullOrWhiteSpace(parsedResponse.Token))
                    SetAccessToken(parsedResponse.Token);
            }

            response.Dispose();
        }

        public void RemoveAccessToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public void CleanRefreshTokens()
        {
            //Find a way to do this
        }

        private Uri MakeUri(Controller controller, string endpointMethod)
        {
            UriBuilder uriBuilder = new UriBuilder(ApiConfig.BaseAddress);

            uriBuilder.Path = $"{controller}/{endpointMethod}";

            return uriBuilder.Uri;
        }

        private Uri MakeUri(Controller controller, string endpointMethod, string queryParams)
        {//Potentially replace this with UriHelper
         //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.extensions.urihelper?view=aspnetcore-5.0

            UriBuilder uriBuilder = new UriBuilder(ApiConfig.BaseAddress);

            uriBuilder.Path = $"{controller}/{endpointMethod}";
            uriBuilder.Query = queryParams;

            return uriBuilder.Uri;
        }

        private void InitializeHttpClient()
        {
            _httpClientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            _httpClient = new HttpClient(_httpClientHandler);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _httpClient.BaseAddress = new Uri(ApiConfig.BaseAddress, UriKind.Absolute);
        }
    }
}
