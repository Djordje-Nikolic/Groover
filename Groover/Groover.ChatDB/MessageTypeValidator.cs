using Groover.ChatDB.Interfaces;
using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB
{
    internal class MessageTypeValidator : IMessageTypeValidator
    {
        public void ValidateImageMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.Image == null)
                throw new ArgumentException("Message image cannot be empty in an image message.", nameof(message.Image));

            if (message.TrackId != null ||
                message.TrackName != null ||
                message.TrackDuration != null)
                throw new ArgumentException("Image message cannot contain a track.", nameof(message.TrackId));
        }

        public void ValidateTextMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (string.IsNullOrWhiteSpace(message.Content))
                throw new ArgumentException("Message content cannot be empty in a text message.", nameof(message.Content));

            if (message.Image != null)
                throw new ArgumentException("Text message cannot contain an image.", nameof(message.Image));

            if (message.TrackId != null ||
                message.TrackName != null ||
                message.TrackDuration != null)
                throw new ArgumentException("Text message cannot contain a track.", nameof(message.TrackId));
        }

        public void ValidateTrackMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.Content != null)
                throw new ArgumentException("Track message cannot contain text content.", nameof(message.Content));

            if (message.Image != null)
                throw new ArgumentException("Track message cannot contain an image.", nameof(message.Image));

            if (message.TrackId == null ||
                string.IsNullOrWhiteSpace(message.TrackName) ||
                message.TrackDuration == null)
                throw new ArgumentException("Message track information cannot be undefined in a track message.", nameof(message.TrackId));
        }
    }
}
