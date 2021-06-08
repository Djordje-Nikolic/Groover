using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.DTOs
{
    public class InvitationDTO
    {
        public UserDTO User { get; set; }
        public GroupDTO Group { get; set; }
        public string InvitationToken { get; set; }
    }
}
