using Cassandra;
using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    public interface ITrackRepository
    {
        Task<Track> AddAsync(Track track);
        Task DeleteAsync(TimeUuid trackId);
        Task DeleteAsync(Track track);
        Task<Track> UpdateAsync(Track track);
        Task<Track> GetAsync(string trackId);
        Task<Track> GetAsync(TimeUuid trackId);
        Task<ICollection<Track>> GetByGroupAsync(int groupId);
        Task<ICollection<Track>> GetByGroupAsync(int groupId, PageParams pageParams);
        Task<ICollection<Track>> GetAfterAsync(int groupId, DateTime afterDateTime);
        Task<ICollection<Track>> GetAfterAsync(int groupId, DateTime afterDateTime, PageParams pageParams);
        Task<bool> LoadAsync(Track track, bool checkHash = false);
    }
}
