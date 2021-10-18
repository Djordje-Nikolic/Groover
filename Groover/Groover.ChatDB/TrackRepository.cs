using Cassandra;
using Cassandra.Mapping;
using Groover.ChatDB.Interfaces;
using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB
{
    public class TrackRepository : ITrackRepository
    {
        private readonly IMapper _mapper;
        private readonly IGroupChatSession _groupChatSession;
        private readonly IModelGetter<Track> _modelGetter;
        private ISession _session { get => _groupChatSession.Session; }

        internal TrackRepository(IGroupChatSession session,
                               IMapper mapper,
                               IModelGetter<Track> modelGetter) : this(session)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _modelGetter = modelGetter ?? throw new ArgumentNullException(nameof(modelGetter));
        }

        public TrackRepository(IGroupChatSession session)
        {
            _groupChatSession = session ?? throw new ArgumentNullException(nameof(session));
            _mapper = new Cassandra.Mapping.Mapper(_session);
            _modelGetter = new ModelGetter<Track>(_mapper);
        }

        public async Task<Track> AddAsync(Track track)
        {
            //Validate
            Validate(track);

            //Generate new TimeUuid
            track.Id = TimeUuid.NewId(DateTime.UtcNow);

            //Add the new row
            await _mapper.InsertAsync(track);
            return track; //Maybe fetch instead
        }

        public async Task DeleteAsync(TimeUuid trackId)
        {
            await _mapper.DeleteAsync<Track>("WHERE trackId = ?", trackId);
        }

        public async Task DeleteAsync(Track track)
        {
            await _mapper.DeleteAsync<Track>(track);
        }

        public async Task<Track> GetAsync(int groupId, string trackId)
        {
            if (string.IsNullOrWhiteSpace(trackId))
                throw new ArgumentNullException(nameof(trackId));

            TimeUuid messageTimeUuid;
            try
            {
                messageTimeUuid = TimeUuid.Parse(trackId);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Argument is not a valid TimeUuid format.", nameof(trackId), e);
            }

            return await GetAsync(groupId, messageTimeUuid);
        }

        public async Task<Track> GetAsync(int groupId, TimeUuid trackId)
        {
            return await _mapper.SingleOrDefaultAsync<Track>("WHERE groupId = ? AND trackId = ?", groupId, trackId);
        }

        public async Task<ICollection<Track>> GetByGroupAsync(int groupId)
        {
            if (groupId < 0)
                throw new ArgumentOutOfRangeException(nameof(groupId));

            var results = await _modelGetter.GetAsync(groupId, "groupId");

            return results;
        }

        public async Task<ICollection<Track>> GetByGroupAsync(int groupId, PageParams pageParams)
        {
            if (groupId < 0)
                throw new ArgumentOutOfRangeException(nameof(groupId));

            var results = await _modelGetter.GetAsync(groupId, "groupId", pageParams);

            return results;
        }

        public async Task<ICollection<Track>> GetAfterAsync(int groupId, DateTime afterDateTime)
        {
            if (groupId < 0)
                throw new ArgumentOutOfRangeException(nameof(groupId));

            var results = await _modelGetter.GetAfterAsync(groupId, "groupId", afterDateTime, "trackId");

            return results;
        }

        public async Task<ICollection<Track>> GetAfterAsync(int groupId, DateTime afterDateTime, PageParams pageParams)
        {
            if (groupId < 0)
                throw new ArgumentOutOfRangeException(nameof(groupId));

            var results = await _modelGetter.GetAfterAsync(groupId, "groupId", afterDateTime, "trackId", pageParams);

            return results;
        }

        public async Task<Track> UpdateAsync(Track track)
        {
            Validate(track);

            await _mapper.UpdateAsync<Track>(track);
            return await GetAsync(track.GroupId, track.Id);
        }

        private void Validate(Track track)
        {
            if (track.Duration < 1)
                throw new ArgumentOutOfRangeException(nameof(track.Duration), "Duration has to be equal or greater than 1.");

            if (string.IsNullOrWhiteSpace(track.Format))
                throw new ArgumentException("Format cannot be undefined.", nameof(track.Format));

            if (string.IsNullOrWhiteSpace(track.Name))
                throw new ArgumentException("Name cannot be undefined.", nameof(track.Name));
        }
    }
}
