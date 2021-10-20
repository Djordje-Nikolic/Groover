using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Groover.AvaloniaUI.Services.Interfaces;
using LibVLCSharp;
using LibVLCSharp.Shared;

namespace Groover.AvaloniaUI.Services
{
    public class VLCWrapper : IVLCWrapper
    {
        private Lazy<LibVLC> _libVLC { get; }
        public LibVLC LibVLC => _libVLC.Value;

        public VLCWrapper(string? libvlcPath = null)
        {
            if (string.IsNullOrWhiteSpace(libvlcPath))
                Core.Initialize();
            else
                Core.Initialize(libvlcPath);

            _libVLC = new Lazy<LibVLC>(() => new LibVLC(), true);
        }

        public MediaPlayer GetPlayer(MediaPlayerRole role)
        {
            var mediaPlayer = new MediaPlayer(LibVLC);
            mediaPlayer.SetRole(role);

            return mediaPlayer;
        }

        public Equalizer GetEqualizer()
        {
            return new Equalizer();
        }

        public Media? GetMedia(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                return null;

            if (!File.Exists(filepath))
                return null;

            return new Media(LibVLC, filepath);
        }
    }
}
