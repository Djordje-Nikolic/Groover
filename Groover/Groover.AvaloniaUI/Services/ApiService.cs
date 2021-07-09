using Groover.AvaloniaUI.Models.Interfaces;
using Groover.AvaloniaUI.Services.Interfaces;
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
        Group
    }

    public class ApiService : IApiService
    {
        private IApiConfiguration _apiConfig;
        private HttpClient _httpClient;
        private HttpClientHandler _httpClientHandler;
        private CookieContainer _cookieContainer { get { return _httpClientHandler.CookieContainer; } }

        public ApiService(IApiConfiguration apiConfiguration)
        {
            _apiConfig = apiConfiguration;

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

        public void RemoveAccessToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private Uri MakeUri(Controller controller, string endpointMethod)
        {
            string uri = $"{controller}/{endpointMethod}";

            return new Uri(uri, UriKind.Relative);
        }

        private Uri MakeUri(Controller controller, string endpointMethod, string queryParams)
        {
            string uri = $"{controller}/{endpointMethod}?{queryParams}";

            return new Uri(uri, UriKind.Relative);
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

            _httpClient.BaseAddress = new Uri(_apiConfig.BaseAddress, UriKind.Absolute);
        }
    }
}
