using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Requests
{
    public class AcceptInvitationRequest
    {
        public string Token { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
    }
}
