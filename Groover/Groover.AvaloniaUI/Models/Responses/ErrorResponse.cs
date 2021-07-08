using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Responses
{
    public class ErrorResponse
    {
        public string Error { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorValue { get; set; }
        public List<string> ErrorCodes { get { return ErrorCode.Split(" ").ToList(); } }
    }
}
