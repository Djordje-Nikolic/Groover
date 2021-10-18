using Groover.ChatDB.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
    public interface ITrackProcessor
    {
        void DeleteImage(string path);
        FileStream GetTrack(string path);
        Task<Track> ProcessTrackAsync(IFormFile trackFile);
        Task<string> SaveTrackAsync(MemoryStream stream);
    }
}
