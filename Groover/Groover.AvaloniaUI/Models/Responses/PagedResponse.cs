using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Responses
{
    public class PagedResponse<T> : BaseResponse where T : class
    {
        public T Data { get; set; }
        public PageParams PageParams { get; set; }
    }
}
