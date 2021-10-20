using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services.Interfaces
{
    public interface IVLCWrapper
    {
        public LibVLC LibVLC { get; }
        public MediaPlayer GetPlayer(MediaPlayerRole role);
        public Equalizer GetEqualizer();
        public Media? GetMedia(string filepath);
    }
}
