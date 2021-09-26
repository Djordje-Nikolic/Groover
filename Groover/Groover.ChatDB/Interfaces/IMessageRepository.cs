using Cassandra;
using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    public interface IMessageRepository : IModelGetter<Message>
    {
        Message Add(Message message);
        bool Delete(TimeUuid messageId);
        bool Delete(Message message);
        Message Update(Message message);
        Message Get(TimeUuid messageId);
    }
}
