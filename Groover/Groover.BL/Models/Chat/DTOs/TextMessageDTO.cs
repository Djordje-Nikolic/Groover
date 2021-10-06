using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Chat.DTOs
{
    public class TextMessageDTO : BaseMessageDTO
    {
        public TextMessageDTO()
        {
            Type = MessageType.Text;
        }

        [Required(AllowEmptyStrings = false)]
        public string Content { get; set; }
    }
}
