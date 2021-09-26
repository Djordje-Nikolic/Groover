using Cassandra;
using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    internal interface ITrackSplitter
    {
        byte[] Join(ICollection<TrackChunk> trackChunks, TimeUuid trackId);
        public ICollection<TrackChunk> Split(byte[] trackBytes, TimeUuid trackId);
    }
}
