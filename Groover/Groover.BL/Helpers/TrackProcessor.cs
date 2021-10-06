using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Groover.BL.Models.Chat;
using ATL;
using Microsoft.AspNetCore.Http;
using System.IO;
using Groover.BL.Models.Exceptions;
using Groover.BL.Models;

namespace Groover.BL.Helpers
{
    public class TrackProcessor : ITrackProcessor
    {
        private AudioConfiguration _config;

        public TrackProcessor(AudioConfiguration audioConfig)
        {
            _config = audioConfig;
        }

        public async Task<ChatDB.Models.Track> ProcessTrackAsync(IFormFile trackFile)
        {
            if (_config.MaxTrackSize < trackFile.Length)
                throw new BadRequestException($"File too big. Max size is: {_config.MaxTrackSize} bytes",
                    $"File too big. Max size is: {_config.MaxTrackSize} bytes", "too_big", _config.MaxTrackSize.ToString());

            string extension = Path.GetExtension(trackFile.FileName)?.Trim('.', ' ');
            if (string.IsNullOrWhiteSpace(extension))
                throw new BadRequestException("No file extension detected.", "No file extension detected.", "bad_extension", _config.AllowedExtensions);

            if (!_config.AllowedExtensionList.Contains(extension))
                throw new BadRequestException($"Extension not supported: {extension}", $"Extension not supported: {extension}", "bad_extension", _config.AllowedExtensions);
            try
            {

                string mimeType = trackFile.ContentType;
                using (MemoryStream ms = new MemoryStream())
                {
                    await trackFile.CopyToAsync(ms);

                    var track = new ATL.Track(ms, mimeType);

                    if (track.AudioFormat == Factory.UNKNOWN_FORMAT)
                        throw new BadRequestException("Couldn't determine the format of the track", "bad_track_format");

                    ChatDB.Models.Track trackMetadata = new();
                    trackMetadata.Duration = (short)track.Duration;
                    trackMetadata.Format = track.AudioFormat.ShortName;
                    trackMetadata.Extension = extension;
                    trackMetadata.Bitrate = track.Bitrate;
                    trackMetadata.TrackBytes = ms.ToArray();

                    return trackMetadata;
                }
            }
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new BadRequestException("Error while working with the track file.", "bad_track_format", e);
            }
        }
    }
}
