using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Chat.DTOs
{
    public abstract class BaseMessageDTO
    {

        public string Id { get; set; }

        [Required]
        public MessageType Type { get; set; }


        public DateTimeOffset CreatedAt { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int SenderId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int GroupId { get; set; }
    }
}
