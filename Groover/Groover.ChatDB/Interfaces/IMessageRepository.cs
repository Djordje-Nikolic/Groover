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
        Task<Message> GetAsync(int groupId, string messageId);
        Task<Message> GetAsync(int groupId, TimeUuid messageId);
        Task<ICollection<Message>> GetByGroupAsync(int groupId);
        Task<ICollection<Message>> GetByGroupAsync(int groupId, PageParams pageParams);
        Task<ICollection<Message>> GetAfterAsync(int groupId, DateTime afterDateTime);
        Task<ICollection<Message>> GetAfterAsync(int groupId, DateTime afterDateTime, PageParams pageParams);
    }
}
