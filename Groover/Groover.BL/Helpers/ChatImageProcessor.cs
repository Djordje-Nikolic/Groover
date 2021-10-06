using Groover.BL.Models;
using Groover.BL.Models.Chat;
using Groover.BL.Models.Exceptions;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
    public class ChatImageProcessor : IChatImageProcessor
    {
        private ChatImageConfiguration _config;

        public ChatImageProcessor(ChatImageConfiguration config)
        {
            _config = config;
        }

        public async Task<byte[]> CheckAsync(MemoryStream ms)
        {
            try
            {
                byte[] imgBytes = ms.ToArray();
                Image image = await Image.LoadAsync(ms);

                if (imgBytes.Length > _config.MaxSizeInBytes)
                    throw new BadRequestException("File too big.", "File too big.", "too_big", errorValue: _config.MaxSizeInBytes.ToString());
                if (image.Width > _config.MaxWidth)
                    throw new BadRequestException("Image too wide.", "Image too wide.", "too_wide", errorValue: _config.MaxWidth.ToString());
                if (image.Width < _config.MinWidth)
                    throw new BadRequestException("Image too narrow.", "Image too narrow.", "too_narrow", errorValue: _config.MinWidth.ToString());
                if (image.Height > _config.MaxHeight)
                    throw new BadRequestException("Image too tall.", "Image too tall.", "too_tall", errorValue: _config.MaxHeight.ToString());
                if (image.Height < _config.MinHeight)
                    throw new BadRequestException("Image too short.", "Image too short.", "too_short", errorValue: _config.MinHeight.ToString());

                return imgBytes;
            }
            catch (BadRequestException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new BadRequestException("Error loading image.", "Image in invalid format.", "bad_format", e);
            }
        }

        public async Task<byte[]> CheckAsync(byte[] imageBytes, bool failOnNull = false)
        {
            if (imageBytes == null)
            {
                if (failOnNull)
                    throw new BadRequestException("Image undefined", "bad_format");

                return null;
            }

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return await CheckAsync(ms);
            }
        }
    }
}
