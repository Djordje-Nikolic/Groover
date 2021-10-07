using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Requests
{
    public class TrackMessageRequest
    {
        public int GroupId { get; set; }

        public string TrackName { get; set; }
    }
}
