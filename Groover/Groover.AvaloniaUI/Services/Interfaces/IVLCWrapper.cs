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
        public MediaPlayer GetPlayer(Media media, MediaPlayerRole role);
        Media? GetMedia(string filepath);
    }
}
