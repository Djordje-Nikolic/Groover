using Cassandra;
using Groover.ChatDB.Interfaces;
using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB
{
    public class GroupChatRepository : IGroupChatRepository
    {
        private readonly ITrackRepository _trackRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageTypeValidator _messageTypeValidator;

        public ITrackRepository TrackRepository { get => _trackRepository; }
        public IMessageRepository MessageRepository { get => _messageRepository; }

        internal GroupChatRepository(ITrackRepository trackRepository,
                                   IMessageRepository messageRepository,
                                   IMessageTypeValidator messageTypeValidator)
        {
            _trackRepository = trackRepository;
            _messageRepository = messageRepository;
            _messageTypeValidator = messageTypeValidator;
        }

        internal GroupChatRepository(IGroupChatSession groupChatSession,
                                     IMessageTypeValidator messageTypeValidator)
        {
            _trackRepository = new TrackRepository(groupChatSession);
            _messageRepository = new MessageRepository(groupChatSession);
            _messageTypeValidator = messageTypeValidator;
        }

        public GroupChatRepository(ITrackRepository trackRepository,
                                   IMessageRepository messageRepository)
        {
            _trackRepository = trackRepository;
            _messageRepository = messageRepository;
            _messageTypeValidator = new MessageTypeValidator();
        }

        public GroupChatRepository(IGroupChatSession groupChatSession)
        {
            _trackRepository = new TrackRepository(groupChatSession);
            _messageRepository = new MessageRepository(groupChatSession);
            _messageTypeValidator = new MessageTypeValidator();
        }

        public async Task<Message> AddTextMessageAsync(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                _messageTypeValidator.ValidateTextMessage(message);
            }
            catch (Exception e)
            {

                throw new ArgumentException("Message is not a valid text message.", nameof(message), e);
            }

            message.Type = MessageType.Text;

            Message addedMessage;
            try
            {
                addedMessage = await _messageRepository.AddAsync(message);
            }
            catch (Exception e)
            {
                throw new Exception("Error occured while adding the message.", e);
            }

            return addedMessage;
        }

        public async Task<Message> AddImageMessageAsync(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                _messageTypeValidator.ValidateImageMessage(message);
            }
            catch (Exception e)
            {

                throw new ArgumentException("Message is not a valid image message.", nameof(message), e);
            }

            message.Type = MessageType.Image;

            Message addedMessage;
            try
            {
                addedMessage = await _messageRepository.AddAsync(message);
            }
            catch (Exception e)
            {
                throw new Exception("Error occured while adding the message.", e);
            }

            return addedMessage;
        }

        public async Task<Message> AddTrackMessageAsync(Message message, Track track)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (track == null)
                throw new ArgumentNullException(nameof(track));

            Track addedTrack;
            try
            {
                addedTrack = await _trackRepository.AddAsync(track);
            }
            catch (Exception e)
            {
                throw new Exception("Error occured while adding the track.", e);
            }

            message.TrackId = addedTrack.Id;
            message.TrackName = addedTrack.Name;
            message.TrackDuration = addedTrack.Duration;
            message.TrackExtension = addedTrack.Extension;

            try
            {
                _messageTypeValidator.ValidateTrackMessage(message);
            }
            catch (Exception e)
            {

                throw new ArgumentException("Message is not a valid track message.", nameof(message), e);
            }

            message.Type = MessageType.Track;

            Message addedMessage;
            try
            {
                addedMessage = await _messageRepository.AddAsync(message);
            }
            catch (Exception e)
            {
                throw new Exception("Error occured while adding the message.", e);
            }

            return addedMessage;
        }

        public async Task<Track> GetLoadedTrackAsync(Message message, bool checkHash = false)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.Type != MessageType.Track)
                throw new ArgumentException("Message is not a Track message.", nameof(message));

            TimeUuid trackId = message.TrackId ?? throw new ArgumentException("TrackId is undefined for this message.", nameof(message));
            Track track = await TrackRepository.GetAsync(trackId);

            if (track == null)
                throw new InvalidOperationException("There is no corresponding Track for this TrackId.");

            var isHashValid = await TrackRepository.LoadAsync(track, checkHash);
            if (!isHashValid && checkHash)
                throw new Exception("Track data is corrupted.");

            return track;
        }
    }
}
