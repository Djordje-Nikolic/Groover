using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Requests
{
    public class UpdateUserRequest
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AvatarBase64 { get; set; }
    }
}
