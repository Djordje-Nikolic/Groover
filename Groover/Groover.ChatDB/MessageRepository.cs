using System;
using Cassandra;
using Groover.ChatDB.Interfaces;

namespace Groover.ChatDB
{
    public class MessageRepository : IMessageRepository
    {
        public MessageRepository(IGroupChatSession session)
        {
            
        }
            
    }
}
