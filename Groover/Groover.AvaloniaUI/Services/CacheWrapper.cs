using Groover.AvaloniaUI.Models.Interfaces;
using Groover.AvaloniaUI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services
{
    public enum FileType
    {
        Generic,
        Image,
        Track
    }

    public class CacheWrapper : ICacheWrapper
    {//TODO: Expand with a dictionary that has pairs of uniqueId and filetype to filepath

        private readonly ICacheConfiguration _config;
        public CacheWrapper(ICacheConfiguration cacheConfiguration)
        {
            _config = cacheConfiguration;
        }

        public async Task<string> CacheFileAsync(Stream stream, string uniqueFilename, FileType fileType = FileType.Generic)
        {
            var fullFilePath = GenerateFilePath(uniqueFilename, fileType);

            if (File.Exists(fullFilePath))
                File.Delete(fullFilePath);
            
            using (FileStream fs = File.Create(fullFilePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fs);
            }

            return fullFilePath;
        }

        public string? LocateCachedFile(string uniqueFilename, FileType fileType)
        {
            var fullFilePath = GenerateFilePath(uniqueFilename, fileType);

            if (File.Exists(fullFilePath))
                return fullFilePath;
            else
                return null;
        }

        public string GenerateFilePath(string uniqueFilename, FileType fileType)
        {
            return Path.Combine(_config.BaseCachePath, fileType.ToString().ToLower(), uniqueFilename);
        }
    }
}
