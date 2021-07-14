using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.DTOs
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; }

        private bool _avatarChanged;
        private byte[] _avatarBytes;
        private Bitmap? _avatarImage;
        public string AvatarBase64
        {
            get { return Convert.ToBase64String(AvatarBytes); }
            set
            {
                AvatarBytes = Convert.FromBase64String(value);
            }
        }
        public byte[] AvatarBytes
        {
            get { return _avatarBytes; }
            set
            {
                _avatarChanged = true;
                _avatarBytes = value;
            }
        }
        public Bitmap? AvatarImage
        {
            get
            {
                if (_avatarChanged)
                {
                    if (AvatarBytes != null && AvatarBytes.Length > 0)
                    {
                        using (var ms = new MemoryStream(AvatarBytes))
                        {
                            _avatarImage = new Bitmap(ms);
                        }
                    }
                    else
                    {
                        _avatarImage = null;
                    }

                    _avatarChanged = false;
                }

                return _avatarImage;
            }
        }
    }
}
