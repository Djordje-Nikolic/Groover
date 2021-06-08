using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Responses
{
    public class GroupUserResponse
    {
        public UserResponse User { get; set; }
        public string GroupRole { get; set; }
    }
}
