using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models
{
    public class AudioConfiguration
    {
        public long MaxTrackSize { get; set; }
        public string TracksDirectoryPath { get; set; }

        private string _allowedExtensions;
        public string AllowedExtensions 
        {
            get => _allowedExtensions;
            set
            {
                _allowedExtensions = value;
                _allowedExtensionList = value.Split(',').Select(ext => ext.Trim()).ToList();
            }
        }

        private IList<string> _allowedExtensionList;
        public ICollection<string> AllowedExtensionList { get => _allowedExtensionList; }
    }
}
