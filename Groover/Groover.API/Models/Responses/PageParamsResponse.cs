using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Responses
{
    public class PageParamsResponse
    {
        public int PageSize { get; set; }
        public string? PagingState { get; set; }
        public string NextPagingState { get; set; }
    }
}
