using System;
using System.Collections.Generic;
using System.IO;
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

        public double MaxSizeInMb { get; set; }
        public double MaxSizeInBytes
        {
            get
            {
                return (double)MaxSizeInMb * 1024 * 1024;
            }
        }

        public string DefaultUserImagePath { get; set; }
        public string DefaultGroupImagePath { get; set; }
        public string ImagesDirectoryPath { get; set; }
    }
}
