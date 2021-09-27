using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using Groover.ChatDB.Interfaces;
using Groover.ChatDB.Models;

namespace Groover.ChatDB
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IMapper _mapper;
        private readonly IGroupChatSession _groupChatSession;
        private readonly IModelGetter<Message> _modelGetter;
        private ISession _session { get => _groupChatSession.Session; }

        public MessageRepository(IGroupChatSession session)
        {
            _groupChatSession = session ?? throw new ArgumentNullException(nameof(session));
            _mapper = new Cassandra.Mapping.Mapper(_session);
            _modelGetter = new ModelGetter<Message>(_mapper);
        }

        public Task<Message> AddAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(TimeUuid messageId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<Message> GetAsync(TimeUuid messageId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Message>> GetAsync(int groupId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Message>> GetAsync(int groupId, PageParams pageParams)
        {
            throw new NotImplementedException();
        }

        public Task<Message> UpdateAsync(Message message)
        {
            throw new NotImplementedException();
        }

        private void Validate(Message message)
        {

        }
    }
}
