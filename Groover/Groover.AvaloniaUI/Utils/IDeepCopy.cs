using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Utils
{
    public interface IDeepCopy<T> where T: class
    {
        T DeepCopy(IMapper mapper);
    }
}
