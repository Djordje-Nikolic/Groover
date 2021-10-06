using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Chat.DTOs
{
    public class PagedDataDTO<T> where T : class
    {
        public T Data { get; set; }
        public PageParamsDTO PageParams { get; set; }
    }
}
