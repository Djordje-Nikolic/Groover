using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Chat.DTOs
{
    public class TrackMessageDTO : BaseMessageDTO
    {
        public TrackMessageDTO()
        {
            Type = MessageType.Track;
        }

        public string TrackId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string TrackName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string TrackFormat { get; set; }

        public short TrackDuration { get; set; }
    }
}
