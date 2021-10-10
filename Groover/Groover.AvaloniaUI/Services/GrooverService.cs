using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        protected ICacheWrapper _cacheWrapper;

        public const int DefaultRetryOnUnauthorizedAttempts = 1;

        public GrooverService(IApiService apiService, ICacheWrapper cacheWrapper)
        {
            _apiService = apiService;
            _cacheWrapper = cacheWrapper;
        }

        public async Task<FileResponse> SendFileRequestAsync(
            Link urlLink,
            string uniqueFilename,
            FileType fileType = FileType.Generic,
            int retryOnUnauthorized = DefaultRetryOnUnauthorizedAttempts)
        {
            HttpRequestMessage message = new HttpRequestMessage(urlLink.GetHttpMethod(), urlLink.Href);

            var response = await _apiService.SendAsync(message);

            //Retry logic
            FileResponse parsedResponse = new FileResponse();
            if (retryOnUnauthorized > 0 &&
                response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _apiService.RefreshTokenAsync();
                parsedResponse = await SendFileRequestAsync(urlLink, uniqueFilename, fileType, retryOnUnauthorized - 1);
            }
            else
            {
                if (response.IsSuccessStatusCode)
                {
                    Stream stream = await response.Content.ReadAsStreamAsync();
                    var filepath = await _cacheWrapper.CacheFileAsync(stream, uniqueFilename, fileType);

                    parsedResponse.FilePath = filepath;
                    parsedResponse.IsSuccessful = true;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    parsedResponse.IsSuccessful = false;
                    ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                    parsedResponse.ErrorCodes = errorResponse?.ErrorCodes ?? new List<string>();
                    parsedResponse.ErrorResponse = errorResponse;
                }
            }

            response.Dispose();
            return parsedResponse;
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

            //Retry logic
            TResponse parsedResponse;
            if (retryOnUnauthorized > 0 &&
                response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _apiService.RefreshTokenAsync();
                parsedResponse = await SendRequestAsync<TResponse>(content, httpMethod, controller, endpointMethod, retryOnUnauthorized - 1);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
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

            TResponse parsedResponse;

            //Retry logic
            if (retryOnUnauthorized > 0 &&
                response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _apiService.RefreshTokenAsync();
                parsedResponse = await SendRequestAsync<TResponse>(queryParameters, httpMethod, controller, endpointMethod, retryOnUnauthorized - 1);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
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
