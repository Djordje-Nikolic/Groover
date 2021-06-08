using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.Exceptions
{
    [Serializable]
    public class GrooverException : Exception
    {
		protected string clientMessage;
		protected string errorCode;

		/// <summary>
		/// Poruka koja se vraca kroz API klijentu.
		/// Obicno ne treba da sadrzi detaljne informacije o gresci.
		/// </summary>
		public string ClientMessage
		{
			get { return this.clientMessage; }
			private set { this.clientMessage = value; }
		}

		public string ErrorCode
		{
			get { return this.errorCode; }
			private set { this.errorCode = value; }
		}

		public GrooverException()
		{
		}

		public GrooverException(string message)
			: base(message)
		{
		}

		public GrooverException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected GrooverException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public GrooverException(string message, string clientMessage)
			: base(message)
		{
			this.clientMessage = clientMessage;
		}

		public GrooverException(string message, string clientMessage, Exception innerException)
			: base(message, innerException)
		{
			this.clientMessage = clientMessage;
		}

		public GrooverException(string message, string clientMessage, string errorCode)
			: base(message)
		{
			this.clientMessage = clientMessage;
			this.errorCode = errorCode;
		}

		public GrooverException(string message, string clientMessage, string errorCode, Exception innerException)
			: base(message, innerException)
		{
			this.clientMessage = clientMessage;
			this.errorCode = errorCode;
		}
	}
}
