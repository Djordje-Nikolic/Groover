using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.DTOs
{
    public enum MessageType
    {
        Text,
        Image,
        Track
    }

    public class Message
    {
        public const string DateTimeFormat = "dd/MM/yyyy HH:mm:ss";
        public string Id { get; set; }
        public string Type { get; set; }
        public string CreatedAt { get; set; }
        public int SenderId { get; set; }
        public int GroupId { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public string TrackId { get; set; }
        public string TrackName { get; set; }
        public short? TrackDuration { get; set; }
    }
}
