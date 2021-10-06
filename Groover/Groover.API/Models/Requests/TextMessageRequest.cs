using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models.Requests
{
    public class TextMessageRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int GroupId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Content { get; set; }
    }
}
