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
        private readonly ITrackSplitter _trackSplitter;
        private readonly ITrackHasher _trackHasher;
        private readonly IModelGetter<Track> _modelGetter;
        private ISession _session { get => _groupChatSession.Session; }
        private int _bytesPerChunk { get => _groupChatSession.Configuration.BytesPerTrackChunk; }

        internal TrackRepository(IGroupChatSession session,
                               IMapper mapper,
                               ITrackSplitter trackSplitter,
                               ITrackHasher trackHasher,
                               IModelGetter<Track> modelGetter) : this(session)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _trackSplitter = trackSplitter ?? throw new ArgumentNullException(nameof(trackSplitter));
            _trackHasher = trackHasher ?? throw new ArgumentNullException(nameof(trackHasher));
            _modelGetter = modelGetter ?? throw new ArgumentNullException(nameof(modelGetter));
        }

        public TrackRepository(IGroupChatSession session)
        {
            _groupChatSession = session ?? throw new ArgumentNullException(nameof(session));
            _mapper = new Cassandra.Mapping.Mapper(_session);
            _trackSplitter = new TrackSplitter(_bytesPerChunk);
            _trackHasher = new MD5TrackHasher();
            _modelGetter = new ModelGetter<Track>(_mapper);
        }

        public async Task<Track> AddAsync(Track track)
        {
            //Validate
            ValidateMetadata(track);
            ValidateTrackBytes(track);

            //Generate new TimeUuid
            track.Id = TimeUuid.NewId(DateTime.UtcNow);

            //Create chunks and add them to the database. If addition fails, abort.
            var chunks = _trackSplitter.Split(track.TrackBytes, track.Id);
            var batch = _mapper.CreateBatch(BatchType.Logged);
            foreach (var chunk in chunks)
                batch.Insert<TrackChunk>(chunk);

            var appliedInfo = await _mapper.ExecuteConditionalAsync<TrackChunk>(batch);
            if (!appliedInfo.Applied)
                throw new InvalidOperationException("Couldn't add all the track chunks to the database. Track addition aborted.");

            //Save metadata (including hash of the whole byte array)
            track.ChunkCount = chunks.Count;
            track.Hash = _trackHasher.GenerateHash(track.TrackBytes);

            //Add the new row
            await _mapper.InsertAsync(track);
            return track; //Maybe fetch instead
        }

        public async Task DeleteAsync(TimeUuid trackId)
        {
            await DeleteChunksAsync(trackId);
            await _mapper.DeleteAsync<Track>("WHERE trackId = ?", trackId);
        }

        public async Task DeleteAsync(Track track)
        {
            await DeleteChunksAsync(track.Id);
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
            ValidateMetadata(track);

            await _mapper.UpdateAsync<Track>(track);
            return await GetAsync(track.GroupId, track.Id);
        }

        public async Task<bool> LoadAsync(Track track, bool checkHash = false)
        {
            if (track == null)
                throw new ArgumentNullException(nameof(track));

            var chunks = (await _mapper.FetchAsync<TrackChunk>("WHERE trackId = ?", track.Id)).ToList();
            if (track.ChunkCount != chunks.Count)
                throw new Exception($"Unexpected chunk count in the database for trackId of {track.Id}.");

            track.TrackBytes = _trackSplitter.Join(chunks, track.Id);

            if (checkHash)
            {
                return _trackHasher.ValidateHash(track.TrackBytes, track.Hash);
            }

            return true;
        }

        private async Task DeleteChunksAsync(TimeUuid trackId)
        {
            await _mapper.DeleteAsync<TrackChunk>("WHERE trackId = ?", trackId);
        }

        private void ValidateMetadata(Track track)
        {
            if (track.Duration < 1)
                throw new ArgumentOutOfRangeException(nameof(track.Duration), "Duration has to be equal or greater than 1.");

            if (string.IsNullOrWhiteSpace(track.Format))
                throw new ArgumentException("Format cannot be undefined.", nameof(track.Format));

            if (string.IsNullOrWhiteSpace(track.Name))
                throw new ArgumentException("Name cannot be undefined.", nameof(track.Name));
        }

        private void ValidateTrackBytes(Track track)
        {
            if (track.TrackBytes == null)
                throw new ArgumentNullException(nameof(track.TrackBytes));

            if (track.TrackBytes.Length == 0)
                throw new ArgumentException("TrackBytes cannot be empty.", nameof(track.TrackBytes));
        }
    }
}
