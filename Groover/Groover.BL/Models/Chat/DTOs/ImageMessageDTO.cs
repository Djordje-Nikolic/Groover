using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Chat.DTOs
{
    public class ImageMessageDTO : BaseMessageDTO
    {
        public ImageMessageDTO()
        {
            Type = MessageType.Image;
        }

        public string Content { get; set; }
        [MinLength(1)]
        [Required]
        public byte[] Image { get; set; }
    }
}
