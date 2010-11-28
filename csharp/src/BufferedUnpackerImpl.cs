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

    internal object UnpackNull()
    {
		More(1);
		int b = buffer[offset] & 0xff;
        if (b != 0xc0) // nil
        {  
			throw new MessageTypeException();
		}
		Advance(1);
		return null;
	}

    internal float UnpackFloat()
    {
        More(1);
        int b = buffer[offset];
        switch (b & 0xff)
        {
            case 0xca: // float
                More(5);
                var f = BitConverter.ToSingle(buffer, offset + 1);
                Advance(5);
                return f;
            case 0xcb: // double
                More(9);
                var d = BitConverter.ToDouble(buffer, offset + 1);
                Advance(9);
                // FIXME overflow check
                return (float) d;
            default:
                throw new MessageTypeException();
        }
    }

    internal double UnpackDouble()
    {
        More(1);
        int b = buffer[offset];
        switch (b & 0xff)
        {
            case 0xca: // float
                More(5);
                var f = BitConverter.ToSingle(buffer, offset + 1);
                Advance(5);
                return f;
            case 0xcb: // double
                More(9);
                var d = BitConverter.ToDouble(buffer, offset + 1);
                Advance(9);
                return d;
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
