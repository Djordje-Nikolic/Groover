using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Exceptions
{
    public class NotFoundException : GrooverException
    {
		public NotFoundException()
		{
		}

		public NotFoundException(string message)
			: base(message)
		{
		}

		public NotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected NotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public NotFoundException(string message, string errorCode)
			: base(message, errorCode)
		{
		}

		public NotFoundException(string message, string clientMessage, Exception innerException)
			: base(message, clientMessage, innerException)
		{
		}

		public NotFoundException(string message, string clientMessage, string errorCode)
			: base(message, clientMessage, errorCode)
		{
		}

		public NotFoundException(string message, string clientMessage, string errorCode, string errorValue)
			: base(message, clientMessage, errorCode, errorValue)
		{
		}

		public NotFoundException(string message, string clientMessage, string errorCode, Exception innerException)
			: base(message, clientMessage, errorCode, innerException)
		{
		}
	}
}
