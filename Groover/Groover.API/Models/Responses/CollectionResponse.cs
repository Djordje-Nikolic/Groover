using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Responses
{
    public class CollectionResponse<T>
    {
        public ICollection<T> Items { get; set; }
    }
}
