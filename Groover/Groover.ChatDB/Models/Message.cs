﻿using Cassandra;
using Cassandra.Mapping.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Models
{
    [Table("messages")]
    public class Message : BaseCassandraModel
    {
        [Column("messageId")]
        [ClusteringKey(ClusteringSortOrder = Cassandra.Mapping.SortOrder.Descending)]
        public TimeUuid Id { get; set; }

        [Column("senderId")]
        public int SenderId { get; set; }

        [Column("groupId")]
        [PartitionKey]
        public int GroupId { get; set; }

        [Column("createdAt")]
        public DateTimeOffset CreatedAt {get; set;}

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
        public short? TrackDuration { get; set; }

        [Column("trackExtension")]
        public string TrackExtension { get; set; }

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

        public void SetTrackId(string timeUuId)
        {
            if (string.IsNullOrWhiteSpace(timeUuId))
                throw new ArgumentNullException(nameof(timeUuId));

            try
            {
                this.TrackId = TimeUuid.Parse(timeUuId);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Argument is not a valid TimeUuid format.", nameof(timeUuId), e);
            }
        }
    }
}
