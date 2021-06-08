using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.DTOs
{
    public class RegisteredDTO
    {
        public UserDTO User { get; set; }
        public string ConfirmationToken { get; set; }
    }
}
