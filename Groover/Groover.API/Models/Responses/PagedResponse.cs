using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Responses
{
    public class PagedResponse<T> where T : class
    {
        public T Data { get; set; }
        public PageParamsResponse PageParams { get; set; }
    }
}
