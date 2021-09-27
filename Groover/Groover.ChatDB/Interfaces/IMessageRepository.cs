using Cassandra;
using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message> AddAsync(Message message);
        Task DeleteAsync(TimeUuid messageId);
        Task DeleteAsync(Message message);
        Task<Message> UpdateAsync(Message message);
        Task<Message> GetAsync(TimeUuid messageId);
        Task<ICollection<Message>> GetAsync(int groupId);
        Task<ICollection<Message>> GetAsync(int groupId, PageParams pageParams);
    }
}
