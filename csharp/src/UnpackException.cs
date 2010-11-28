using System.IO;

public class UnpackException : IOException
{
    public UnpackException(string message) : base(message)
    {
    }
}