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
    internal class TrackSplitter : ITrackSplitter
    {
        public readonly int _bytesPerChunk;

        public TrackSplitter(int bytesPerChunk)
        {
            if (bytesPerChunk < 1)
                throw new ArgumentOutOfRangeException(nameof(bytesPerChunk));

            _bytesPerChunk = bytesPerChunk;
        }

        public ICollection<TrackChunk> Split(byte[] trackBytes, TimeUuid trackId)
        {
            if (trackBytes == null)
                throw new ArgumentNullException(nameof(trackBytes));

            if (trackBytes.Length == 0)
                throw new ArgumentException("TrackBytes cannot be empty.", nameof(trackBytes));

            int currentByte = 0;
            int currentChunk = 0;
            List<TrackChunk> trackChunks = new List<TrackChunk>((int)Math.Ceiling((double)trackBytes.Length / _bytesPerChunk));

            while (currentByte < trackBytes.Length)
            {
                int bytesInChunk = Math.Min(trackBytes.Length - currentByte, _bytesPerChunk);
                byte[] bytesForChunk = new byte[bytesInChunk];

                Array.Copy(trackBytes, currentByte, bytesForChunk, 0, bytesInChunk);

                TrackChunk trackChunk = new TrackChunk();
                trackChunk.ChunkOrder = currentChunk;
                trackChunk.TrackId = trackId;
                trackChunk.Chunk = bytesForChunk;
                trackChunks.Add(trackChunk);

                currentByte += bytesInChunk;
                currentChunk++;
            }

            return trackChunks;
        }

        public byte[] Join(ICollection<TrackChunk> trackChunks, TimeUuid trackId)
        {
            if (trackChunks == null)
                throw new ArgumentNullException(nameof(trackChunks));

            if (trackChunks.Count == 0)
                throw new ArgumentException("TrackChunks cannot be empty.", nameof(trackChunks));

            ICollection<TrackChunk> sortedChunks = trackChunks.OrderBy(chunk => chunk.ChunkOrder).ToList();
            int totalLength = trackChunks.Sum(chunk => chunk.Chunk.Length);
            byte[] joinedTrack = new byte[totalLength];

            int currentLength = 0;
            int expectedChunk = 0;

            foreach (var chunk in sortedChunks)
            {
                if (chunk.ChunkOrder != expectedChunk)
                    throw new InvalidOperationException($"Chunk number {expectedChunk} is missing.");

                if (chunk.TrackId != trackId)
                    throw new InvalidOperationException("Chunk is part of the wrong track.");

                Array.Copy(chunk.Chunk, 0, joinedTrack, currentLength, chunk.Chunk.Length);

                expectedChunk++;
                currentLength += chunk.Chunk.Length;
            }

            return joinedTrack;
        }
    }
}
