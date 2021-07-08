using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Utils
{
    public static class Extensions
    {
        public static void ForEach<T>(this ICollection<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }
    }
}
