using Groover.AvaloniaUI.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Responses
{
    public class LoginResponse : BaseResponse
    {
        public User User { get; set; }
        public string Token { get; set; }
    }
}
