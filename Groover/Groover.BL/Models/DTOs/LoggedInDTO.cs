﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.DTOs
{
    public class LoggedInDTO
    {
		public UserDTO User { get; set; }
		public string Token { get; set; }
		public string RefreshToken { get; set; }
	}
}
