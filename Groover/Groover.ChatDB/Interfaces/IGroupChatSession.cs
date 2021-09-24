using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    public interface IGroupChatSession
    {
        public Cassandra.ISession Session { get; }
    }
}
