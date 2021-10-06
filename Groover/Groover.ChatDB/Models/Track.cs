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
    public class Track : BaseCassandraModel
    {
        [Column("trackId")]
        [ClusteringKey(ClusteringSortOrder = Cassandra.Mapping.SortOrder.Descending)]
        public TimeUuid Id { get; set; }

        [Column("groupId")]
        [PartitionKey]
        public int GroupId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("duration")]
        public short Duration { get; set; }

        [Column("format")]
        public string Format { get; set; }

        [Column("bitrate")]
        public int Bitrate { get; set; }

        [Column("extension")]
        public string Extension { get; set; }

        [Column("hash")]
        public string Hash { get; set; }

        [Column("chunkCount")]
        internal int ChunkCount { get; set; }

        [Ignore]
        public byte[] TrackBytes { get; set; }

        public void SetId(string timeUuId)
        {
            if (string.IsNullOrWhiteSpace(timeUuId))
                throw new ArgumentNullException(nameof(timeUuId));

            try
            {
                this.Id = TimeUuid.Parse(timeUuId);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Argument is not a valid TimeUuid format.", nameof(timeUuId), e);
            }
        }
    }
}
