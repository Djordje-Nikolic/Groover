﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Responses
{
    public class UserGroupResponse
    {
        public GroupLiteResponse Group { get; set; }
        public string GroupRole { get; set; }
    }
}
