using Cassandra;
using Cassandra.Mapping.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Models
{
    [Table("tracksMetadata")]
    public class Track
    {
        [Column("trackId")]
        [ClusteringKey(ClusteringSortOrder = Cassandra.Mapping.SortOrder.Descending)]
        public TimeUuid Id { get; set; }

        [Column("groupId")]
        [PartitionKey]
        public long GroupId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("duration")]
        public short Duration { get; set; }

        [Column("format")]
        public string Format { get; set; }

        [Column("hash")]
        public string Hash { get; set; }

        [Column("chunkCount")]
        public int ChunkCount { get; set; }
    }
}
