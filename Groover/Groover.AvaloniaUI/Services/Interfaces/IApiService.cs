using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services.Interfaces
{
    public interface IApiService
    {
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message,
            Controller controller,
            string endpointMethod,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead);

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message,
            Controller controller,
            string endpointMethod,
            string queryParams,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead);

        public void SetAccessToken(string token, string type = "Bearer");
        public void RemoveAccessToken();
        public void CleanRefreshTokens();
    }
}
