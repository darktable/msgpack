using System;

namespace MsgPack {
	public class MessagePackException : Exception
	{
	    public MessagePackException()
	    {
	    }

	    public MessagePackException(Exception innerException)
	        : base("MessagePack error", innerException)
	    {
	    }

	    public MessagePackException(string message)
	        : base(message)
	    {
	    }

	    public MessagePackException(string message, Exception innerException)
	        : base(message, innerException)
	    {
	    }
	}
}