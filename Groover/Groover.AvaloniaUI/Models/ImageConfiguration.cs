using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models
{
    public class ImageConfiguration
    {
        public int MaxWidth { get; set; }
        public int MinWidth { get; set; }
        public int MaxHeight { get; set; }
        public int MinHeight { get; set; }
        public double MaxSizeInMb { get; set; }

        private List<string> _allowedExtensions;
        public ICollection<string> AllowedExtensions => _allowedExtensions;

        public ImageConfiguration(NameValueCollection nvC)
        {
            if (nvC == null)
                throw new ArgumentNullException();

            MaxWidth = int.Parse(nvC["MaxWidth"] ?? "0");
            MaxHeight = int.Parse(nvC["MaxHeight"] ?? "0");
            MinWidth = int.Parse(nvC["MinWidth"] ?? "0");
            MinHeight = int.Parse(nvC["MinHeight"] ?? "0");
            MaxSizeInMb = double.Parse(nvC["MaxSizeInMb"] ?? "0");
            string allExtensions = nvC["AllowedExtensions"] ?? "";
            _allowedExtensions = allExtensions.Split(',').Select(ext => ext.Trim()).ToList();
        }
    }
}
