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
        // FIXME overflow check
        return (float) UnpackDouble();
    }

    internal double UnpackDouble()
    {
        More(1);
        byte b = buffer[offset];
        switch (b & 0xff)
        {
            case 0xca:
                return UnpackFloatExact();
            case 0xcb:
                return UnpackDoubleExact();
            default:
                throw new MessageTypeException();
        }
    }

    private double UnpackDoubleExact()
    {
        More(9);
        Advance(9);
        return BitConverter.ToDouble(buffer, offset - 8);
    }

    private double UnpackFloatExact()
    {
        More(5);
        Advance(5);
        return BitConverter.ToSingle(buffer, offset - 4);
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
        byte b = buffer[offset];
        if ((b & 0xe0) == 0xa0)  
        {
            // fix raw
            Advance(1);
            return b & 0x1f;
        }
        switch (b & 0xff)
        {
            case 0xda:
                return UnpackUInt16Exact();
            case 0xdb:
                // FIXME overflow check
                return (int) UnpackUInt32Exact();
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

    internal ulong UnpackUlong()
    {
        More(1);
        byte b = buffer[offset];
        if ((b & 0x80) == 0) // positive fixnum
        {  
            Advance(1);
            return b;
        }
        switch (b & 0xff)
        {
            case 0xcc:
                return UnpackUInt8Exact();
            case 0xcd:
                return UnpackUInt16Exact();
            case 0xce: 
                return UnpackUInt32Exact();
            case 0xcf: 
                return UnpackUInt64Exact();
            default:
                throw new MessageTypeException();
        }
    }

    private ulong UnpackUInt8Exact()
    {
        More(2);
        Advance(2);
        return buffer[offset - 1];
    }

    private ushort UnpackUInt16Exact()
    {
        More(3);
        Advance(3);
        return BitConverter.ToUInt16(buffer, offset - 2);
    }

    private uint UnpackUInt32Exact()
    {
        More(5);
        Advance(5);
        return BitConverter.ToUInt32(buffer, offset - 4);        
    }

    private ulong UnpackUInt64Exact()
    {
        More(9);
        Advance(9);
        return BitConverter.ToUInt64(buffer, offset - 8); 
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