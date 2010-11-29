using System;
using System.Text;

public class BufferedUnpackerImpl : UnpackerImpl
{
    private readonly Func<bool> fillCallback;

    internal byte[] buffer;
    internal int filled;
    internal int offset;

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

    internal bool TryUnpackNull()
    {
        if (!TryMore(1))
        {
            return false;
        }
        int b = buffer[offset] & 0xff;
        if (b != 0xc0) // nil
        {
            return false;
        }
        Advance(1);
        return true;
    }

    internal float UnpackFloat()
    {
        More(1);
        int b = buffer[offset];
        switch (b & 0xff)
        {
            case 0xca: // float
                More(5);
                float f = BitConverter.ToSingle(buffer, offset + 1);
                Advance(5);
                return f;
            case 0xcb: // double
                More(9);
                double d = BitConverter.ToDouble(buffer, offset + 1);
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
                float f = BitConverter.ToSingle(buffer, offset + 1);
                Advance(5);
                return f;
            case 0xcb: // double
                More(9);
                double d = BitConverter.ToDouble(buffer, offset + 1);
                Advance(9);
                return d;
            default:
                throw new MessageTypeException();
        }
    }

    internal string UnpackString()
    {
        if (TryUnpackNull())
        {
            return null;
        }
        int length = UnpackRaw();
        More(length);
        String s;
        try
        {
            s = Encoding.UTF8.GetString(buffer, offset, length);
        }
        catch (Exception e)
        {
            throw new MessageTypeException(e);
        }
        Advance(length);
        return s;
    }

    internal int UnpackRaw()
    {
        More(1);
        int b = buffer[offset];
        if ((b & 0xe0) == 0xa0)  // fix raw
        {
            Advance(1);
            return b & 0x1f;
        }
        int len;
        switch (b & 0xff)
        {
            case 0xda: // raw 16
                More(3);
                len = BitConverter.ToUInt16(buffer, offset + 1);
                Advance(3);
                return len;
            case 0xdb: // raw 32
                More(5);
                // FIXME overflow check
                len = (int) BitConverter.ToUInt32(buffer, offset + 1);
                Advance(5);
                return len;
            default:
                throw new MessageTypeException();
        }
    }

    internal byte[] UnpackRawBody(int length)
    {
        More(length);
        var bytes = new byte[length];
        Array.Copy(buffer, offset, bytes, 0, length);
        Advance(length);
        return bytes;
    }

    private void Advance(int length)
    {
        offset += length;
    }

    private void More(int require)
    {
        if (!TryMore(require))
        {
            // FIXME
            throw new UnpackException("insufficient buffer");
        }
    }

    private bool TryMore(int require)
    {
        while (filled - offset < require)
        {
            if (!fillCallback())
            {
                return false;
            }
        }
        return true;
    }
}