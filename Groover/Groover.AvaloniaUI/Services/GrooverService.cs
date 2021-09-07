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

        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(
            TRequest request,
            HttpMethod httpMethod,
            Controller controller,
            string endpointMethod, 
            int retryOnUnauthorized = DefaultRetryOnUnauthorizedAttempts) where TResponse : BaseResponse, new()
        {
            HttpRequestMessage message = new HttpRequestMessage(httpMethod, string.Empty);
            var json = JsonConvert.SerializeObject(request);
            message.Content = new StringContent(json, Encoding.UTF8, "application/json");

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
                await SendRequestAsync<TRequest, TResponse>(request, httpMethod, controller, endpointMethod, retryOnUnauthorized - 1);
            }

            return parsedResponse;
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(
                IDictionary<string, string> queryParameters,
                HttpMethod httpMethod,
                Controller controller,
                string endpointMethod,
                int retryOnUnauthorized = DefaultRetryOnUnauthorizedAttempts) where TResponse : BaseResponse, new()
        {
            HttpRequestMessage message = new HttpRequestMessage(httpMethod, string.Empty);

            var queryParams = BuildQuery(queryParameters);
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
                await SendRequestAsync<TResponse>(queryParameters, httpMethod, controller, endpointMethod, retryOnUnauthorized - 1);
            }

            return parsedResponse;
        }

        private string BuildQuery(IDictionary<string, string> queryParams)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (queryParams.Count == 0)
                return string.Empty;

            var lastPair = queryParams.Last();
            foreach (var keyValuePair in queryParams)
            {
                stringBuilder.Append($"{keyValuePair.Key}={keyValuePair.Value}");

                if (!keyValuePair.Equals(lastPair))
                {
                    stringBuilder.Append("&");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
