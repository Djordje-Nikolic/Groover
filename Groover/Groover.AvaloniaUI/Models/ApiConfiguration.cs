using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Groover.AvaloniaUI.Models.Interfaces;

namespace Groover.AvaloniaUI.Models
{
    public class ApiConfiguration : IApiConfiguration
    {
        public string BaseAddress { get; set; }
        public string APIKey { get; set; }

        public ApiConfiguration(NameValueCollection nvC)
        {
            if (nvC == null)
                throw new ArgumentNullException();

            BaseAddress = nvC["BaseAddress"];
            APIKey = nvC["APIKey"];
        }
    }
}
