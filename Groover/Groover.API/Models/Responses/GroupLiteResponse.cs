using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Responses
{
    public class GroupLiteResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageBase64 { get; set; }
        public ICollection<GroupUserLiteResponse> GroupUsers { get; set; }
    }
}
