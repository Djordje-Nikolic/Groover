using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Requests
{
    public class SetGroupImageRequest
    {
        public int GroupId { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
