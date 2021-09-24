using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    public interface IChatDbConfiguration
    {
        public string ContactPointAddress { get; set; }
        public string GroupChatKeySpace { get; set; }
    }
}
