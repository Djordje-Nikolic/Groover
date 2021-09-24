using Groover.ChatDB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB
{
    public class ChatDbConfiguration : IChatDbConfiguration
    {
        public string ContactPointAddress { get; set; }
        public string GroupChatKeySpace { get; set; }
    }
}
