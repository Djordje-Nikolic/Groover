using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.DTOs
{
    public class RegisterDTO
    {
		[Required(AllowEmptyStrings =false)]
		public string Username { get; set; }
		[Required(AllowEmptyStrings = false)]
		public string Password { get; set; }
		[Required(AllowEmptyStrings = false)]
		public string Email { get; set; }
	}
}
