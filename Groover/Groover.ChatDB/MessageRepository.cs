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

        internal MessageRepository(IGroupChatSession groupChatSession,
                                   IModelGetter<Message> modelGetter) : this(groupChatSession)
        {
            _modelGetter = modelGetter;
        }

        public MessageRepository(IGroupChatSession session)
        {
            _groupChatSession = session ?? throw new ArgumentNullException(nameof(session));
            _mapper = new Cassandra.Mapping.Mapper(_session);
            _modelGetter = new ModelGetter<Message>(_mapper);
        }

        public async Task<Message> AddAsync(Message message)
        {
            //Validate
            Validate(message);

            //Generate new TimeUuid
            message.CreatedAt = DateTime.UtcNow;
            message.Id = TimeUuid.NewId(message.CreatedAt);

            await _mapper.InsertAsync(message);
            return message;
        }

        public async Task DeleteAsync(TimeUuid messageId)
        {
            await _mapper.DeleteAsync<Message>("WHERE messageId = ?", messageId);
        }

        public async Task DeleteAsync(Message message)
        {
            await _mapper.DeleteAsync<Message>(message);
        }

        public async Task<Message> GetAsync(int groupId, string messageId)
        {
            if (string.IsNullOrWhiteSpace(messageId))
                throw new ArgumentNullException(nameof(messageId));

            TimeUuid messageTimeUuid;

            try
            {
                messageTimeUuid = TimeUuid.Parse(messageId);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Argument is not a valid TimeUuid format.", nameof(messageId), e);
            }

            return await GetAsync(groupId, messageTimeUuid);
        }

        public async Task<Message> GetAsync(int groupId, TimeUuid messageId)
        {
            return await _mapper.SingleOrDefaultAsync<Message>("WHERE groupId = ? AND messageId = ?", groupId, messageId);
        }

        public async Task<ICollection<Message>> GetByGroupAsync(int groupId)
        {
            if (groupId < 0)
                throw new ArgumentOutOfRangeException(nameof(groupId));

            var results = await _modelGetter.GetAsync(groupId, "groupId");

            return results;
        }

        public async Task<ICollection<Message>> GetByGroupAsync(int groupId, PageParams pageParams)
        {
            if (groupId < 0)
                throw new ArgumentOutOfRangeException(nameof(groupId));

            var results = await _modelGetter.GetAsync(groupId, "groupId", pageParams);

            return results;
        }

        public async Task<ICollection<Message>> GetAfterAsync(int groupId, DateTime afterDateTime)
        {
            if (groupId < 0)
                throw new ArgumentOutOfRangeException(nameof(groupId));

            var results = await _modelGetter.GetAfterAsync(groupId, "groupId", afterDateTime, "messageId");

            return results;
        }

        public async Task<ICollection<Message>> GetAfterAsync(int groupId, DateTime afterDateTime, PageParams pageParams)
        {
            if (groupId < 0)
                throw new ArgumentOutOfRangeException(nameof(groupId));

            var results = await _modelGetter.GetAfterAsync(groupId, "groupId", afterDateTime, "messageId", pageParams);

            return results;
        }

        public async Task<Message> UpdateAsync(Message message)
        {
            Validate(message);

            await _mapper.UpdateAsync<Message>(message);
            return await GetAsync(message.GroupId, message.Id);
        }

        private void Validate(Message message)
        {
            if (message.SenderId < 1)
                throw new ArgumentOutOfRangeException(nameof(message.SenderId));

            if (message.GroupId < 1)
                throw new ArgumentOutOfRangeException(nameof(message.GroupId));

            if (message.Type == null)
                throw new ArgumentException("MessageType cannot be undefined.", nameof(message.Type));

            if (message.Image != null && message.Image.Length > _groupChatSession.Configuration.MaximumImageSizeInBytes)
                throw new ArgumentException($"Image size exceeded maximum allowed bytes: {_groupChatSession.Configuration.MaximumImageSizeInBytes}", nameof(message.Image));
        }
    }
}
