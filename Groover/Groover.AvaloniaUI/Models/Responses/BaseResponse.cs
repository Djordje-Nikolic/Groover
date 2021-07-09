using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Responses
{
    public class BaseResponse
    {
        public string? Message { get; set; }
        public bool IsSuccessful { get; set; }
        public List<string> ErrorCodes { get; set; }
        public ErrorResponse ErrorResponse { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
