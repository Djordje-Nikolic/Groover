using Cassandra;
using Cassandra.Mapping.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Models
{
    [Table("tracksChunks")]
    internal class TrackChunk
    {
        [Column("trackId")]
        [PartitionKey]
        public TimeUuid TrackId { get; set; }

        [Column("chunkOrder")]
        [ClusteringKey(ClusteringSortOrder = Cassandra.Mapping.SortOrder.Ascending)]
        public int ChunkOrder { get; set; }

        [Column("chunk")]
        public byte[] Chunk { get; set; }
    }
}
