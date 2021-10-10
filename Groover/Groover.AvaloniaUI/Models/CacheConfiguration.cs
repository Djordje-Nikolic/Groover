using Groover.AvaloniaUI.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models
{
    internal class CacheConfiguration : ICacheConfiguration
    {
        public string BaseCachePath { get; set; }

        public CacheConfiguration(NameValueCollection nvC)
        {
            if (nvC == null)
                throw new ArgumentNullException();

            BaseCachePath = nvC["BaseCachePath"];
        }
    }
}
