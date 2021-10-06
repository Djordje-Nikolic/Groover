using Groover.ChatDB.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
    public interface ITrackProcessor
    {
        Task<Track> ProcessTrackAsync(IFormFile trackFile);
    }
}
