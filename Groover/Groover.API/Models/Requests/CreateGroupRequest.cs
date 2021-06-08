using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Requests
{
    public class CreateGroupRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
