namespace MsgPack {
	public class MessagePackOverflowException: MessagePackException
	{
	    public MessagePackOverflowException(string type)
	        : base("Too large value for " + type + ".")
	    {
	    }
	}
}