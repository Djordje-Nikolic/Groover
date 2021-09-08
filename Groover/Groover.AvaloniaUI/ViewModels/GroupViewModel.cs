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
    public class GroupViewModel : ViewModelBase, IDeepCopy<GroupViewModel>
    {
        [Reactive]
        public int Id { get; set; }
        [Reactive]
        public string Name { get; set; }
        [Reactive]
        public string Description { get; set; }

        public SourceCache<GroupUserViewModel, int> GroupUsersCache { get; private set; }
        private ReadOnlyObservableCollection<GroupUserViewModel> _sortedGroupUsers;
        public ReadOnlyObservableCollection<GroupUserViewModel> SortedGroupUsers => _sortedGroupUsers;

        private byte[] _imageBytes;
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
                this.RaiseAndSetIfChanged(ref _imageBytes, value);
            }
        }

        [ObservableAsProperty]
        public Bitmap? Image { get; }

        public GroupViewModel()
        {
            this.WhenAnyValue(group => group.ImageBytes)
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
                .ToPropertyEx(this, group => group.Image);

            GroupUsersCache = new SourceCache<GroupUserViewModel, int>(gu => gu.User.Id);
            GroupUsersCache.Connect()
                .AutoRefresh(groupUserViewModel => groupUserViewModel.GroupRole)
                .Sort(SortExpressionComparer<GroupUserViewModel>.Descending(groupUserViewModel => groupUserViewModel.GroupRole))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _sortedGroupUsers)
                .Subscribe();     
        }

        public GroupViewModel DeepCopy(IMapper mapper)
        {
            Group serialized = mapper.Map<Group>(this);
            GroupViewModel copy = mapper.Map<GroupViewModel>(serialized);
            return copy;
        }
    }
}
