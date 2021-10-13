using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Requests
{
    public class ImageMessageRequest
    {
        public ImageMessageRequest(string imageFilePath)
        {
            if (string.IsNullOrWhiteSpace(imageFilePath))
                throw new ArgumentNullException(nameof(imageFilePath));

            if (File.Exists(imageFilePath))
            {
                var imageBytes = File.ReadAllBytes(imageFilePath);

                if (imageBytes != null && imageBytes.Length > 0)
                    Image = Convert.ToBase64String(imageBytes);
            }
        }

        public int GroupId { get; set; }

        public string Content { get; set; }

        public string Image { get; set; }
    }
}
