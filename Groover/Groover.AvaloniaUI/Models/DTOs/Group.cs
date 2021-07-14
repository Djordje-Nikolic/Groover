using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.DTOs
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<GroupUser> GroupUsers { get; set; }

        private bool _imageChanged;
        private byte[] _imageBytes;
        private Bitmap? _image;
        public string ImageBase64
        {
            get 
            {
                return ImageBytes != null ? Convert.ToBase64String(ImageBytes) : null;
            }
            set
            {
                ImageBytes = value != null ? Convert.FromBase64String(value) : null;
            }
        }
        public byte[] ImageBytes
        {
            get { return _imageBytes; }
            set
            {
                _imageChanged = true;
                _imageBytes = value;
            }
        }
        public Bitmap? Image
        {
            get
            {
                if (_imageChanged)
                {
                    if (ImageBytes != null && ImageBytes.Length > 0)
                    {
                        using (var ms = new MemoryStream(ImageBytes))
                        {
                            _image = new Bitmap(ms);
                        }
                    }
                    else
                    {
                        _image = null;
                    }

                    _imageChanged = false;
                }

                return _image;
            }
        }
    }
}
