using Avalonia.Media.Imaging;
using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services;
using Groover.AvaloniaUI.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels
{
    public class MessageViewModel : ViewModelBase
    {
        private readonly Func<string, Task<TrackResponse>> _trackLoadDelegate;

        public string Id { get; set; }
        public MessageType Type { get; set; }
        public DateTime CreatedAt { get; set; }

        public GroupUserViewModel GroupUser { get; }

        [ObservableAsProperty]
        public UserViewModel User { get; set; }
        [ObservableAsProperty]
        public GrooverGroupRole Role { get; set; }
        [ObservableAsProperty]
        public Bitmap? Image { get; set; }

        [Reactive]
        public string Content { get; set; }
        [Reactive]
        public byte[] ImageBytes { get; set; }

        public string TrackId { get; set; }
        public string TrackName { get; set; }
        public short? TrackDuration { get; set; }

        public ReactiveCommand<Unit, Unit> LoadTrackCommand { get; }

        public MessageViewModel(Message message, 
            GroupUserViewModel groupUser,
            Func<string, Task<TrackResponse>> trackLoadDelegate)
        {
            _trackLoadDelegate = trackLoadDelegate;

            Id = message.Id;

            if (!Enum.TryParse(typeof(MessageType), message.Type, out object? tempMessageType) || tempMessageType == null)
            {
                throw new ArgumentException("Type couldn't be parsed.", nameof(message));
            }
            else
            {
                Type = (MessageType)tempMessageType;
            }

            if (!DateTime.TryParseExact(message.CreatedAt,
                Message.DateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out DateTime dateTimeResult))
            {
                throw new ArgumentException("CreatedAt couldn't be parsed.", nameof(message));
            }
            else
            {
                CreatedAt = dateTimeResult.ToLocalTime();
            }

            if (message.SenderId != groupUser.User.Id)
                throw new ArgumentException("Invalid user passed as argument.");

            Content = message.Content;
            TrackId = message.TrackId;
            TrackName = message.TrackName;
            TrackDuration = message.TrackDuration;

            this.WhenAnyValue(mVm => mVm.GroupUser.User)
                .ToPropertyEx(this, mVm => mVm.User);

            this.WhenAnyValue(mVm => mVm.GroupUser.GroupRole)
                .ToPropertyEx(this, mVm => mVm.Role);

            //Idk if this will actually trigger stuff TEST, maybe manual assignment needed through OnUserRoleUpdated
            GroupUser = groupUser;

            this.WhenAnyValue(mVm => mVm.ImageBytes)
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
                .ToPropertyEx(this, mVm => mVm.Image);

            ImageBytes = string.IsNullOrWhiteSpace(message.Image) ? null : Convert.FromBase64String(message.Image);

            var canLoadTrack = this.WhenAnyValue(vm => vm.Type, type => type == MessageType.Track);
            LoadTrackCommand = ReactiveCommand.CreateFromTask(LoadTrack, canLoadTrack);
        }

        private async Task LoadTrack()
        {
            var trackResponse = await _trackLoadDelegate.Invoke(this.TrackId);
            //Load track
        }
    }
}
