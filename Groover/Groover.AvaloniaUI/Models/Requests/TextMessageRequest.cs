using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Requests
{
    public class TextMessageRequest
    {
        public int GroupId { get; set; }

        public string Content { get; set; }

        public TextMessageRequest() { }

        public TextMessageRequest(string textContent) 
        {
            if (string.IsNullOrWhiteSpace(textContent))
                throw new ArgumentNullException(nameof(textContent));

            Content = textContent;
        }
    }
}
