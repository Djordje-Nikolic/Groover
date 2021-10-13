﻿using Avalonia.Media.Imaging;
using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Responses;
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
using LibVLCSharp;
using System.Collections.ObjectModel;
using DynamicData;

namespace Groover.AvaloniaUI.ViewModels.Chat
{
    public class MessageViewModel : ViewModelBase, IDisposable
    {
        public const string DefaultDateTimeDisplayFormat = "HH:mm";
        public const string DefaultFullDateTimeDisplayFormat = "dddd, dd MMMM yyyy HH:mm:ss";

        private readonly Func<string, Task<TrackResponse>> _trackLoadDelegate;
        private readonly IVLCWrapper _vlcWrapper;
        private bool disposedValue;

        public string Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool SentByLoggedInUser { get; private set; }

        public GroupUserViewModel GroupUser { get; }

        [Reactive]
        public MessageType Type { get; private set; }
        [ObservableAsProperty]
        public bool HasTrack { get; }
        [ObservableAsProperty]
        public bool HasImage { get; }
        [ObservableAsProperty]
        public bool HasText { get; }

        [ObservableAsProperty]
        public string CreatedAtDisplay { get; }
        [ObservableAsProperty]
        public string FullCreatedAtDisplay { get; }
        [Reactive]
        public TrackViewModel? Track { get; private set; }
        [ObservableAsProperty]
        public UserViewModel User { get; }
        [ObservableAsProperty]
        public GrooverGroupRole Role { get; }
        [ObservableAsProperty]
        public Bitmap? Image { get; }
        [Reactive]
        public string? DisplayError { get; private set; }

        [Reactive]
        public string? Content { get; private set; }
        [Reactive]
        public byte[] ImageBytes { get; private set; }

        public string TrackId { get; private set; }
        public string TrackName { get; private set; }
        public short? TrackDuration { get; private set; }

        public MessageViewModel(Message message,
            GroupUserViewModel groupUser,
            IVLCWrapper vlcWrapper,
            bool sentByLoggedInUser,
            Func<string, Task<TrackResponse>> trackLoadDelegate,
            string dateTimeDisplayFormat = DefaultDateTimeDisplayFormat)
        {
            _trackLoadDelegate = trackLoadDelegate;
            _vlcWrapper = vlcWrapper;
            SentByLoggedInUser = sentByLoggedInUser;

            Id = message.Id;

            if (!Enum.TryParse(typeof(MessageType), message.Type, out object? tempMessageType) || tempMessageType == null)
            {
                throw new ArgumentException("Type couldn't be parsed.", nameof(message));
            }
            else
            {
                Type = (MessageType)tempMessageType;
            }

            //Converting message type to bindable values for the frontend
            this.WhenAnyValue(mVm => mVm.Type)
                .Select(type => type == MessageType.Track)
                .ToPropertyEx(this, mVm => mVm.HasTrack);

            this.WhenAnyValue(mVm => mVm.Type)
                .Select(type => type == MessageType.Image)
                .ToPropertyEx(this, mVm => mVm.HasImage);

            this.WhenAnyValue(mVm => mVm.Type)
                .Select(type => type == MessageType.Text ||
                                type == MessageType.Image)
                .ToPropertyEx(this, mVm => mVm.HasText);

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

            this.WhenAnyValue(mVm => mVm.CreatedAt)
                .Select(dt => dt.ToString(dateTimeDisplayFormat))
                .ToPropertyEx(this, mVm => mVm.CreatedAtDisplay);

            this.WhenAnyValue(mVm => mVm.CreatedAt)
                .Select(dt => dt.ToString(DefaultFullDateTimeDisplayFormat))
                .ToPropertyEx(this, mVm => mVm.FullCreatedAtDisplay);

            if (message.SenderId != groupUser.User.Id)
                throw new ArgumentException("Invalid user passed as argument.");

            Content = message.Content;

            //Init user and role data
            this.WhenAnyValue(mVm => mVm.GroupUser.User)
                .ToPropertyEx(this, mVm => mVm.User);

            this.WhenAnyValue(mVm => mVm.GroupUser.GroupRole)
                .ToPropertyEx(this, mVm => mVm.Role);

            //Idk if this will actually trigger stuff TEST, maybe manual assignment needed through OnUserRoleUpdated
            GroupUser = groupUser;

            //Init image data
            this.WhenAnyValue(mVm => mVm.ImageBytes)
                .Select(bytes =>
                {
                    if (bytes != null && bytes.Length > 0)
                    {
                        try
                        {
                            using (var ms = new MemoryStream(bytes))
                            {
                                return new Bitmap(ms);
                            }
                        }
                        catch (Exception)
                        {
                            DisplayError = "Error loading the image.";
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                })
                .ToPropertyEx(this, mVm => mVm.Image);

            ImageBytes = string.IsNullOrWhiteSpace(message.Image) ? null : Convert.FromBase64String(message.Image);

            TrackId = message.TrackId;
            TrackName = message.TrackName;
            TrackDuration = message.TrackDuration;

            //Init track data
            if (Type == MessageType.Track)
            {
                Track = GetInitialTrackViewModel();
            }
        }

        private TrackViewModel? GetInitialTrackViewModel()
        {
            if (Type == MessageType.Track)
                return null;

            try
            {
                var tVm = new TrackViewModel(TrackId, TrackName, TrackDuration ?? 0, _trackLoadDelegate, _vlcWrapper);

                return tVm;
            }
            catch (Exception)
            {
                DisplayError = "Error loading the track.";
                return null;
            }
        }

        #region Dispose Pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (Track != null)
                        Track.Dispose();
                    Track = null;

                    if (Image != null)
                        Image.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MessageViewModel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
