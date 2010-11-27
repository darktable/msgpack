using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Unpacker : IEnumerable<MessagePackObject>
{
    private readonly Stream inputStream;

    public Unpacker(Stream inputStream)
    {
        this.inputStream = inputStream;
    }

    public IEnumerator<MessagePackObject> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
