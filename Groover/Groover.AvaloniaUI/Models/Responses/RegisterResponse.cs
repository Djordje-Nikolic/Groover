using Groover.AvaloniaUI.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Responses
{
    public class RegisterResponse : BaseResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; }
    }
}
