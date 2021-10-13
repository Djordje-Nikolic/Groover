using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models
{
    public class TrackConfiguration
    {
        public long MaxTrackSize { get; }
        public int MaxNameLength { get; }

        private List<string> _allowedExtensions;
        public ICollection<string> AllowedExtensions => _allowedExtensions;

        public TrackConfiguration(NameValueCollection nvC)
        {
            if (nvC == null)
                throw new ArgumentNullException();

            MaxNameLength = int.Parse(nvC["MaxNameLength"] ?? "100");
            MaxTrackSize = long.Parse(nvC["MaxTrackSize"] ?? "0");
            string allExtensions = nvC["AllowedExtensions"] ?? "";
            _allowedExtensions = allExtensions.Split(',').Select(ext => ext.Trim()).ToList();
        }
    }
}
