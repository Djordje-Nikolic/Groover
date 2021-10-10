using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Responses
{
    public class CollectionResponse<T> : BaseResponse
    {
        public ICollection<T> Items { get; set; }
    }
}
