using Cassandra;
using Cassandra.Mapping.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Models
{
    [Table("messages")]
    public class Message
    {
        [Column("messageId")]
        [ClusteringKey(ClusteringSortOrder = Cassandra.Mapping.SortOrder.Descending)]
        public TimeUuid Id { get; set; }

        [Column("senderId")]
        public long SenderId { get; set; }

        [Column("groupId")]
        [PartitionKey]
        public long GroupId { get; set; }

        [Column("type")]
        public string type { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("image")]
        public byte[] Image { get; set; }

        [Column("trackId")]
        public TimeUuid? TrackId { get; set; }

        [Column("trackName")]
        public string TrackName { get; set; }

        [Column("trackDuration")]
        public short TrackDuration { get; set; }

        [Ignore]
        public MessageType? Type
        {
            get
            {
                if (Enum.TryParse<MessageType>(type, out MessageType typeValue))
                {
                    return typeValue;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                    type = "";
                else
                {
                    type = value.ToString();
                }
            }
        }

    }
}
