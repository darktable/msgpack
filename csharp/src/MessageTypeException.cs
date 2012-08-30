using System;

namespace MsgPack{
public class MessageTypeException: MessagePackException
{
    public MessageTypeException()
        : base("Type error.")
    {
    }

    public MessageTypeException(Exception innerException)
        : base("Type error", innerException)
    {
    }

    public MessageTypeException(string message) : base(message)
    {
    }
}
}