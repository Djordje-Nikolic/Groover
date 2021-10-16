using DynamicData;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using LibVLCSharp.Shared;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Chat
{
    public class TrackViewModel : ViewModelBase, IDisposable
    {
        private readonly IVLCWrapper _vlcWrapper;
        private readonly ISourceList<string> _errorList;
        private readonly Func<string, Task<TrackResponse>> _loadTrackResponseDelegate; 
        private bool disposedValue;
        private bool _registeredHandlers;
        private long _elapsedMiliseconds;
        private int _currentVolume;

        public string Id { get; private set; }

        public int GroupId { get; private set; }

        public string Name { get; private set; }

        public string Format { get; private set; }

        public string ContentType { get; private set; }

        public string Extension { get; private set; }

        public string Hash { get; private set; }

        public string TrackFilePath { get; private set; }

        public Media TrackMedia { get; private set; }

        public MediaPlayer MediaPlayer { get; private set; }

        [ObservableAsProperty]
        public bool Loaded { get; }
        public int CurrentVolume
        {
            get => _currentVolume;
            set => VolumeChanged(value, true);
        }
        public long ElapsedMiliseconds
        {
            get => _elapsedMiliseconds;
            set => TimeChanged(value, true);
        }
        [ObservableAsProperty]
        public TimeSpan ElapsedTime { get; }
        [Reactive]
        public long TotalDurationMs { get; private set; }
        [Reactive]
        public TimeSpan TotalDuration { get; private set; }
        [Reactive]
        public bool Muted { get; private set; }
        [ObservableAsProperty]
        public string? AllErrors { get; }

        public ReactiveCommand<Unit, Unit> PlayCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleMuteCommand { get; }
        public ReactiveCommand<bool, bool> LoadOrResetCommand { get; }

        private TrackViewModel(Func<string, Task<TrackResponse>> loadResponseDelegate, IVLCWrapper vlcWrapper)
        {
            _vlcWrapper = vlcWrapper;
            _loadTrackResponseDelegate = loadResponseDelegate;

            _errorList = new SourceList<string>();
            _errorList.Connect()
                .ToCollection()
                .Select(x => string.Join(Environment.NewLine, x))
                .ToPropertyEx(this, tVm => tVm.AllErrors, initialValue: null);

            this.WhenAnyValue(tVm => tVm.ElapsedMiliseconds)
                .Select(value => TimeSpan.FromMilliseconds(value))
                .ToPropertyEx(this, tVm => tVm.ElapsedTime);

            Loaded = false;
            CurrentVolume = 100;
            var isLoaded = this.WhenAnyValue(tVm => tVm.Loaded, loaded => loaded == true);
            PlayCommand = ReactiveCommand.Create(PlayTrack, isLoaded);
            StopCommand = ReactiveCommand.Create(StopTrack, isLoaded);
            PauseCommand = ReactiveCommand.Create(PauseTrack, isLoaded);
            ToggleMuteCommand = ReactiveCommand.Create(ToggleTrackMute, isLoaded);

            LoadOrResetCommand = ReactiveCommand.CreateFromTask<bool,bool>(async (load) => 
            {
                if (load)
                    return await Load();
                else
                    return Reset();
            });
            LoadOrResetCommand.ToPropertyEx(this, tVm => tVm.Loaded);

            this.WhenAnyValue(tVm => tVm.Loaded)
                .Where(loaded => loaded == true)
                .Subscribe(_ => RegisterEventHandlers());
        }

        public TrackViewModel(string id, string name, short duration, Func<string, Task<TrackResponse>> loadResponseDelegate, IVLCWrapper vlcWrapper) : this(loadResponseDelegate, vlcWrapper)
        {
            if (string.IsNullOrWhiteSpace(id))
                _errorList.Add("Invalid track id.");

            if (string.IsNullOrWhiteSpace(name))
                _errorList.Add("Invalid track name.");

            if (duration < 0)
                _errorList.Add("Invalid track duration.");

            Id = id;
            Name = name;
            SetTrackDuration(duration);
        }

        public async Task<bool> Load()
        {
            if (Loaded)
                return true;

            var track = await GetResponse();
            if (track == null)
                return false;

            Id = track.Id;
            GroupId = track.GroupId;
            Name = track.Name;
            Format = track.Format;
            ContentType = track.ContentType;
            Extension = track.Extension;
            Hash = track.Hash;


            if (track.TrackFilePath == null)
            {
                _errorList.Add("Track file path missing.");
                return false;
            }
            else
            {
                TrackFilePath = track.TrackFilePath;
            }

            try
            {
                var media = _vlcWrapper.GetMedia(TrackFilePath);
                if (media == null)
                {
                    _errorList.Add("Couldn't open Media object.");
                    return false;
                }
                else
                {
                    await media.Parse(timeout: 2000);
                    if (media.IsParsed == false)
                    {
                        media.Dispose();

                        _errorList.Add("Couldn't parse media metadata.");
                        return false;
                    }

                    TrackMedia = media;
                    SetTrackDuration(media.Duration);
                    MediaPlayer = _vlcWrapper.GetPlayer(TrackMedia, MediaPlayerRole.Music);

                    return true;
                }
            }
            catch (Exception e)
            {
                _errorList.Add($"Error loading track: {e.Message}");
            }

            return false;
        }

        private async Task<TrackResponse?> GetResponse()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                _errorList.Add($"Track Id is missing.");
                return null;
            }

            try
            {
                var trackResponse = await _loadTrackResponseDelegate.Invoke(Id);

                if (trackResponse.IsComplete)
                {
                    return trackResponse;
                }
                else
                {
                    if (trackResponse.IsSuccessful)
                    {
                        if (trackResponse.TrackFileResponse.ErrorCodes == null)
                            _errorList.Add($"Error loading track.");
                        else
                        {
                            foreach (var code in trackResponse.TrackFileResponse.ErrorCodes.Distinct())
                            {
                                switch (code)
                                {
                                    case "not_member":
                                        _errorList.Add("User is not a member of the group.");
                                        break;
                                    case "bad_id":
                                        _errorList.Add("Bad group id.");
                                        break;
                                    case "bad_uuid":
                                        _errorList.Add("Bad track uuid.");
                                        break;
                                    case "not_found_group":
                                        _errorList.Add("Group not found.");
                                        break;
                                    case "not_found":
                                        _errorList.Add("Track not found.");
                                        break;
                                    case "internal":
                                    default:
                                        _errorList.Add("Unknown error occured.");
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (trackResponse.ErrorCodes == null)
                            _errorList.Add("Error loading track metadata.");
                        else
                        {
                            foreach (var code in trackResponse.ErrorCodes.Distinct())
                            {
                                switch (code)
                                {
                                    case "not_member":
                                        _errorList.Add("User is not a member of the group.");
                                        break;
                                    case "bad_id":
                                        _errorList.Add("Bad group id.");
                                        break;
                                    case "bad_uuid":
                                        _errorList.Add("Bad track uuid.");
                                        break;
                                    case "not_found_group":
                                        _errorList.Add("Group not found.");
                                        break;
                                    case "not_found":
                                        _errorList.Add("Track not found.");
                                        break;
                                    case "internal":
                                    default:
                                        _errorList.Add("Unknown error occured.");
                                        break;
                                }
                            }
                        }
                    }
                    
                    return null;
                }
            }
            catch (Exception e)
            {
                _errorList.Add($"Error loading track: {e.Message}");
            }

            return null;
        }

        private bool Reset()
        {
            if (MediaPlayer != null)
                MediaPlayer.Dispose();

            if (TrackMedia != null)
                TrackMedia.Dispose();

            MediaPlayer = null;
            TrackMedia = null;
            _registeredHandlers = false;

            return false;
        }

        private void SetTrackDuration(long miliseconds)
        {
            TotalDurationMs = miliseconds;
            TotalDuration = TimeSpan.FromMilliseconds(miliseconds);
        }

        private void SetTrackDuration(short seconds)
        {
            TotalDurationMs = seconds * 1000;
            TotalDuration = TimeSpan.FromSeconds(seconds);
        }

        private void TimeChanged(long newElapsedMs, bool sendToPlayer)
        {
            if (sendToPlayer)
                ChangePlayerTrackTime(newElapsedMs);

            this.RaiseAndSetIfChanged(ref _elapsedMiliseconds, newElapsedMs, nameof(ElapsedMiliseconds));
        }

        private void VolumeChanged(int newVolume, bool sendToPlayer)
        {
            if (sendToPlayer)
                ChangePlayerTrackVolume(newVolume);

            this.RaiseAndSetIfChanged(ref _currentVolume, newVolume, nameof(CurrentVolume));
        }

        #region MediaPlayer Methods
        private void RegisterEventHandlers()
        {
            if (_registeredHandlers)
                return;

            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MediaPlayer.Muted += (s, e) => MediaPlayer_MuteToggled(s, e, true);
            MediaPlayer.Unmuted += (s, e) => MediaPlayer_MuteToggled(s, e, false);
            MediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
            _registeredHandlers = true;
        }

        private void MediaPlayer_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            TimeChanged(e.Time, false);
        }

        private void MediaPlayer_VolumeChanged(object? sender, MediaPlayerVolumeChangedEventArgs e)
        {
            VolumeChanged((int)(e.Volume * 100), false);
        }

        private void MediaPlayer_MuteToggled(object? sender, EventArgs e, bool muted)
        {
            Muted = muted;
        }

        private void ChangePlayerTrackTime(long newTime)
        {
            if (MediaPlayer != null)
            {
                MediaPlayer.Time = newTime;
            }
        }

        private void ChangePlayerTrackVolume(int newVolume)
        {
            if (MediaPlayer != null)
            {
                MediaPlayer.Volume = newVolume;
            }
        }

        private void PlayTrack()
        {
            MediaPlayer.Play();
        }

        private void PauseTrack()
        {
            MediaPlayer.Pause();
        }

        private void StopTrack()
        {
            MediaPlayer.Stop();
            ElapsedMiliseconds = 0;
        }

        private void ToggleTrackMute()
        {
            MediaPlayer.ToggleMute();
        }
        #endregion

        #region Dispose Pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    LoadOrResetCommand.Execute(false).Subscribe();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TrackViewModel()
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
