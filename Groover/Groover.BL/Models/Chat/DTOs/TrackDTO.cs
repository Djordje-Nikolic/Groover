using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Chat.DTOs
{
    public class TrackDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int GroupId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Range(1, short.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public short Duration { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Format { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ContentType { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Extension { get; set; }

        public FileStream TrackStream { get; set; }
    }
}
