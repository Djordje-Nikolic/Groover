using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Requests
{
    public class ImageMessageRequest
    {
        public int GroupId { get; set; }

        public string Content { get; set; }

        public string Image { get; set; }
    }
}
