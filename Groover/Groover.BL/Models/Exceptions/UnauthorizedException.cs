using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Exceptions
{
    public class UnauthorizedException : GrooverException
    {
		public UnauthorizedException()
		{
		}

		public UnauthorizedException(string message)
			: base(message)
		{
		}

		public UnauthorizedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected UnauthorizedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public UnauthorizedException(string message, string errorCode)
			: base(message, errorCode)
		{
		}

		public UnauthorizedException(string message, string clientMessage, Exception innerException)
			: base(message, clientMessage, innerException)
		{
		}

		public UnauthorizedException(string message, string clientMessage, string errorCode)
			: base(message, clientMessage, errorCode)
		{
		}

		public UnauthorizedException(string message, string clientMessage, string errorCode, Exception innerException)
			: base(message, clientMessage, errorCode, innerException)
		{
		}
	}
}
