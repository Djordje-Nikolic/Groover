using Cassandra;
using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    public interface ITrackRepository : IModelGetter<Track>
    {
        Task<Track> AddAsync(Track track);
        Task DeleteAsync(TimeUuid trackId);
        Task DeleteAsync(Track track);
        Task<Track> UpdateAsync(Track track);
        Task<Track> GetAsync(TimeUuid trackId);
        Task<bool> LoadAsync(Track track, bool checkHash = false);
    }
}
