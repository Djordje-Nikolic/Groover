using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB
{
    public class PageParams
    {
        public int PageSize { get; set; } = 10;
        public string PagingState { get; set; } = null;
        public string NextPagingState { get; set; }

        public static byte[] ConvertPagingState(string pagingState)
        {
            if (pagingState == null)
                return null;

            return Convert.FromBase64String(pagingState);
        }

        public static string ConvertPagingState(byte[] pagingState)
        {
            if (pagingState == null)
                return null;

            return Convert.ToBase64String(pagingState);
        }
    }
}
