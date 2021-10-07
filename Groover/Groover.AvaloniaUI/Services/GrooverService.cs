using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services
{
    public class GrooverService
    {
        protected IApiService _apiService;

        public const int DefaultRetryOnUnauthorizedAttempts = 1;

        public GrooverService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(
            HttpContent content,
            HttpMethod httpMethod,
            Controller controller,
            string endpointMethod,
            int retryOnUnauthorized = DefaultRetryOnUnauthorizedAttempts) where TResponse : BaseResponse, new()
        {
            HttpRequestMessage message = new HttpRequestMessage(httpMethod, string.Empty);
            message.Content = content;

            var response = await _apiService.SendAsync(message, controller, endpointMethod);
            var responseContent = await response.Content.ReadAsStringAsync();

            TResponse parsedResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                parsedResponse = JsonConvert.DeserializeObject<TResponse>(responseContent);
                parsedResponse.IsSuccessful = true;
            }
            else
            {
                parsedResponse = new TResponse() { IsSuccessful = false };
                ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                parsedResponse.ErrorCodes = errorResponse?.ErrorCodes ?? new List<string>();
                parsedResponse.ErrorResponse = errorResponse;
            }

            parsedResponse.StatusCode = response.StatusCode;

            //Retry logic
            if (retryOnUnauthorized > 0 &&
                parsedResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _apiService.RefreshTokenAsync();
                parsedResponse = await SendRequestAsync<TResponse>(content, httpMethod, controller, endpointMethod, retryOnUnauthorized - 1);
            }

            response.Dispose();
            return parsedResponse;
        }

        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(
            TRequest request,
            HttpMethod httpMethod,
            Controller controller,
            string endpointMethod, 
            int retryOnUnauthorized = DefaultRetryOnUnauthorizedAttempts) where TResponse : BaseResponse, new()
        {          
            var json = JsonConvert.SerializeObject(request);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            return await this.SendRequestAsync<TResponse>(content, httpMethod, controller, endpointMethod, retryOnUnauthorized);
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(
                IDictionary<string, string> queryParameters,
                HttpMethod httpMethod,
                Controller controller,
                string endpointMethod,
                int retryOnUnauthorized = DefaultRetryOnUnauthorizedAttempts) where TResponse : BaseResponse, new()
        {
            HttpRequestMessage message = new HttpRequestMessage(httpMethod, string.Empty);

            var queryParams = await BuildQuery(queryParameters);
            var response = await _apiService.SendAsync(message, controller, endpointMethod, queryParams);
            var responseContent = await response.Content.ReadAsStringAsync();

            TResponse parsedResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                parsedResponse = JsonConvert.DeserializeObject<TResponse>(responseContent);
                parsedResponse.IsSuccessful = true;
            }
            else
            {
                parsedResponse = new TResponse() { IsSuccessful = false };
                ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                parsedResponse.ErrorCodes = errorResponse?.ErrorCodes ?? new List<string>();
                parsedResponse.ErrorResponse = errorResponse;
            }

            parsedResponse.StatusCode = response.StatusCode;

            //Retry logic
            if (retryOnUnauthorized > 0 &&
                parsedResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _apiService.RefreshTokenAsync();
                parsedResponse = await SendRequestAsync<TResponse>(queryParameters, httpMethod, controller, endpointMethod, retryOnUnauthorized - 1);
            }

            response.Dispose();
            return parsedResponse;
        }

        private async Task<string> BuildQuery(IDictionary<string, string> queryParams)
        {
            var query = await new FormUrlEncodedContent(queryParams).ReadAsStringAsync();

            return query;
        }
    }
}
