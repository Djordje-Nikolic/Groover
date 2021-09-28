using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    internal interface IMessageTypeValidator
    {
        public void ValidateTextMessage(Message message);
        public void ValidateImageMessage(Message message);
        public void ValidateTrackMessage(Message message);
    }
}
