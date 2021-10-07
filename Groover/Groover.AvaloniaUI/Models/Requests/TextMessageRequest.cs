using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Requests
{
    public class TextMessageRequest
    {
        public int GroupId { get; set; }

        public string Content { get; set; }
    }
}
