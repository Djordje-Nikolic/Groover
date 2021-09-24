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
        internal TimeUuid TrackId { get; set; }

        [Column("chunkOrder")]
        [ClusteringKey(ClusteringSortOrder = Cassandra.Mapping.SortOrder.Ascending)]
        internal int ChunkOrder { get; set; }

        [Column("chunk")]
        internal byte[] Chunk { get; set; }
    }
}
