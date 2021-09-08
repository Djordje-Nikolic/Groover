using AutoMapper;
using Avalonia.Media.Imaging;
using DynamicData;
using DynamicData.Binding;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels
{
    public class UserViewModel : ViewModelBase, IDeepCopy<UserViewModel>
    {
        [Reactive]
        public int Id { get; set; }
        [Reactive]
        public string Username { get; set; }
        [Reactive]
        public string Email { get; set; }

        public SourceCache<UserGroupViewModel, int> UserGroupsCache { get; private set; }
        private ReadOnlyObservableCollection<UserGroupViewModel> _userGroups;
        public ReadOnlyObservableCollection<UserGroupViewModel> UserGroups => _userGroups;

        [Reactive]
        public bool IsOnline { get; set; }

        private byte[] _avatarBytes;
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
        public byte[] AvatarBytes
        {
            get { return _avatarBytes; }
            set
            {
                this.RaiseAndSetIfChanged(ref _avatarBytes, value);
            }
        }

        [ObservableAsProperty]
        public Bitmap? AvatarImage { get; }

        public UserViewModel()
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

            UserGroupsCache = new SourceCache<UserGroupViewModel, int>(ug => ug.Group.Id);
            UserGroupsCache.Connect()
                .AutoRefresh(userGroupViewModel => userGroupViewModel.GroupRole)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _userGroups)
                .Subscribe();
        }

        public UserViewModel DeepCopy(IMapper mapper)
        {
            User serialized = mapper.Map<User>(this);
            UserViewModel copy = mapper.Map<UserViewModel>(serialized);
            return copy;
        }
    }
}
