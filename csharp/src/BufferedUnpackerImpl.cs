using System;

public class BufferedUnpackerImpl: UnpackerImpl
{
    private readonly Func<bool> fillCallback;

    internal int offset;
    internal int filled;
    internal byte[] buffer;

    public BufferedUnpackerImpl(Func<bool> fillCallback)
    {
        this.fillCallback = fillCallback;
    }

    internal bool UnpackBool()
    {
		More(1);
		int b = buffer[offset] & 0xff;
		switch (b)
		{
		    case 0xc2:
		        Advance(1);
		        return false;
		    case 0xc3:
		        Advance(1);
		        return true;
		    default:
		        throw new MessageTypeException();
		}
	}

    private void Advance(int length) {
		offset += length;
	}

    private void More(int require) 
    {
		while (filled - offset < require) 
        {
			if(!fillCallback()) 
            {
				// FIXME
				throw new UnpackException("insufficient buffer");
			}
		}
	}
}
