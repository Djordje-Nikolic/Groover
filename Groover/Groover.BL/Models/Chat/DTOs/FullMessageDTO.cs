using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Chat.DTOs
{
    public class FullMessageDTO : BaseMessageDTO
    {
        public string Content { get; set; }
        public byte[] Image { get; set; }
        public string TrackId { get; set; }
        public string TrackName { get; set; }
        public short? TrackDuration { get; set; }
    }
}
