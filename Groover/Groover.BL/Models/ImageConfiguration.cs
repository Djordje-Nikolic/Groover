using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models
{
    public class ImageConfiguration
    {
        public int MinWidth { get; set; }
        public int MaxWidth { get; set; }
        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }
        public double MaxSizeInMb { get; set; }
        public string AllowedExtensions { get; set; }
        public List<string> AllowedExtensionsList 
        { 
            get 
            { 
                return AllowedExtensions.Split(",", 
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList(); 
            } 
        }
        public double MaxSizeInBytes
        {
            get
            {
                return (double)MaxSizeInMb * 1024 * 1024;
            }
        }
    }
}
