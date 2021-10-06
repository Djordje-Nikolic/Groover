using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models
{
    public class AvatarImageConfiguration : ImageConfiguration
    {
        public string DefaultUserImagePath { get; set; }
        public string DefaultGroupImagePath { get; set; }
        public string ImagesDirectoryPath { get; set; }
    }
}
