using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Responses
{
    public class TrackResponse
    {
        public string Id { get; set; }

        public int GroupId { get; set; }

        public string Name { get; set; }

        public short Duration { get; set; }

        public string Format { get; set; }

        public string ContentType { get; set; }

        public string Extension { get; set; }

        public string Hash { get; set; }

        public Link TrackFileLink { get; set; }
    }
}
