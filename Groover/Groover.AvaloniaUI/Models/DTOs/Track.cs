using Groover.AvaloniaUI.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.DTOs
{
    public class Track
    {
        public string Id { get; set; }

        public int GroupId { get; set; }

        public string Name { get; set; }

        public short Duration { get; set; }

        public string Format { get; set; }

        public string ContentType { get; set; }

        public string Extension { get; set; }

        public string Hash { get; set; }

        public byte[] TrackBytes { get; set; }
    }
}
