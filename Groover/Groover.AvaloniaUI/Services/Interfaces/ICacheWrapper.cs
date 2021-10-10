using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services.Interfaces
{
    public interface ICacheWrapper
    {
        Task<string> CacheFileAsync(Stream stream, string uniqueId, FileType fileType = FileType.Generic);
        string? LocateCachedFile(string uniqueFilename, FileType fileType);
    }
}
