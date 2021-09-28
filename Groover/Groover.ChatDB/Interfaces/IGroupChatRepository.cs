using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    public interface IGroupChatRepository
    {
        public ITrackRepository TrackRepository { get; }
        public IMessageRepository MessageRepository { get; }

        Task<Message> AddImageMessageAsync(Message message);
        Task<Message> AddTextMessageAsync(Message message);
        Task<Message> AddTrackMessageAsync(Message message, Track track);
        Task<Track> GetLoadedTrackAsync(Message message, bool checkHash = false);
    }
}
