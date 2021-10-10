using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models
{
    public class PageParams
    {
        public int PageSize { get; set; }
        public string? PagingState { get; set; }
        public string? NextPagingState { get; set; }
    }
}
