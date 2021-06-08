using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Requests
{
    public class ConfirmEmailRequest
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}
