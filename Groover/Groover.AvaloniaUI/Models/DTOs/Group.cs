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
    public class Group : ReactiveObject, IDeepCopy<Group>
    {
        [Reactive]
        [DataMember]
        public int Id { get; set; }
        [Reactive]
        [DataMember]
        public string Name { get; set; }
        [Reactive]
        [DataMember]
        public string Description { get; set; }

        [Reactive]
        [DataMember]
        public ObservableCollection<GroupUser> GroupUsers { get; set; }

        [IgnoreDataMember]
        private byte[] _imageBytes;
        [DataMember]
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
        [IgnoreDataMember]
        public byte[] ImageBytes
        {
            get { return _imageBytes; }
            set
            {
                this.RaiseAndSetIfChanged(ref _imageBytes, value);
            }
        }

        [IgnoreDataMember]
        [ObservableAsProperty]
        public Bitmap? Image { get; }

        public Group()
        {
            this.WhenAnyValue(user => user.ImageBytes)
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
                .ToPropertyEx(this, user => user.Image);
        }

        public Group DeepCopy(IMapper mapper)
        {
            GroupRequest serialized = mapper.Map<GroupRequest>(this);
            Group copy = mapper.Map<Group>(serialized);
            return copy;
        }
    }
}
