using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Responses
{
    public class UserLiteResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string AvatarBase64 { get; set; }
    }
}
