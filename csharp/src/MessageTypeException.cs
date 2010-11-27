using System;

public class MessageTypeException: Exception
{
    public MessageTypeException()
    {
    }

    public MessageTypeException(string message) : base(message)
    {
    }

    public MessageTypeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
