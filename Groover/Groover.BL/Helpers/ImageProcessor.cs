using Groover.BL.Models;
using Groover.BL.Models.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
    public class ImageProcessor : IImageProcessor
    {
        private ImageConfiguration _config;

        public ImageProcessor(ImageConfiguration config)
        {
            _config = config;
        }

        public async Task<byte[]> Process(IFormFile imageFile)
        {
            var uploadedFileName = imageFile.FileName;
            var extension = Path.GetExtension(uploadedFileName).ToLowerInvariant().Trim('.');
            if (!_config.AllowedExtensionsList.Contains(extension))
                throw new BadRequestException("Extension is not allowed.", "Extension is not allowed.", errorCode: "invalid_extension", errorValue: _config.AllowedExtensions);

            if (imageFile.Length > _config.MaxSizeInBytes)
                throw new BadRequestException("File too big.", "File too big.", "too_big", errorValue: _config.MaxSizeInBytes.ToString());

            try
            {
                byte[] imgBytes;
                Image image;
                using (MemoryStream ms = new MemoryStream())
                {
                    await imageFile.CopyToAsync(ms);
                    imgBytes = ms.ToArray();
                    image = Image.FromStream(ms);
                }

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
    }
}
