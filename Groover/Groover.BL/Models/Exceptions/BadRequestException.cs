using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Exceptions
{
    public class BadRequestException : GrooverException
    {
		public BadRequestException()
		{
		}

		public BadRequestException(string message)
			: base(message)
		{
		}

		public BadRequestException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected BadRequestException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public BadRequestException(string message, string clientMessage)
			: base(message, clientMessage)
		{
		}

		public BadRequestException(string message, string clientMessage, Exception innerException)
			: base(message, clientMessage, innerException)
		{
		}

		public BadRequestException(string message, string clientMessage, string errorCode)
			: base(message, clientMessage, errorCode)
		{
		}

		public BadRequestException(string message, string clientMessage, string errorCode, Exception innerException)
			: base(message, clientMessage, errorCode, innerException)
		{
		}
	}
}
