using AutoMapper;
using Avalonia.Media.Imaging;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.DTOs
{
    [DataContract]
    public class User : ReactiveObject, IDeepCopy<User>
    {
        [Reactive]
        [DataMember]
        public int Id { get; set; }
        [Reactive]
        [DataMember]
        public string Username { get; set; }
        [Reactive]
        [DataMember]
        public string Email { get; set; }
        [Reactive]
        [DataMember]
        public ObservableCollection<UserGroup> UserGroups { get; set; }

        [Reactive]
        [IgnoreDataMember]
        public bool IsOnline { get; set; }

        [IgnoreDataMember]
        private byte[] _avatarBytes;
        [DataMember]
        public string AvatarBase64
        {
            get
            {
                return AvatarBytes != null ? Convert.ToBase64String(AvatarBytes) : null;
            }
            set
            {
                AvatarBytes = value != null ? Convert.FromBase64String(value) : null;
            }
        }
        [IgnoreDataMember]
        public byte[] AvatarBytes
        {
            get { return _avatarBytes; }
            set
            {
                this.RaiseAndSetIfChanged(ref _avatarBytes, value);
            }
        }

        [IgnoreDataMember]
        [ObservableAsProperty]
        public Bitmap? AvatarImage { get; }

        public User()
        {
            this.WhenAnyValue(user => user.AvatarBytes)
                .Select(bytes =>
                {
                    if (bytes != null && bytes.Length > 0)
                    {
                        using (var ms = new MemoryStream(bytes))
                        {
                            return new Bitmap(ms);
                        }
                    }
                    else
                    {
                        return null;
                    }
                })
                .ToPropertyEx(this, user => user.AvatarImage);
        }

        public User DeepCopy(IMapper mapper)
        {
            UserRequest serialized = mapper.Map<UserRequest>(this);
            User copy = mapper.Map<User>(serialized);
            return copy;
        }
    }
}
