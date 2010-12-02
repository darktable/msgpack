using System;
using System.Text;

public class BufferedUnpackerImpl : UnpackerImpl
{
    private const int BoolFalseType = 0xc2;
    private const int BoolTrueType = 0xc3;
    private const int NilType = 0xc0;
    private const int FloatType = 0xca;
    private const int DoubleType = 0xcb;
    private const int Raw16Type = 0xda;
    private const int Raw32Type = 0xdb;
    private const int UInt8Type = 0xcc;
    private const int UInt16Type = 0xcd;
    private const int UInt32Type = 0xce;
    private const int UInt64Type = 0xcf;
    private const int Int8Type = 0xd0;
    private const int Int16Type = 0xd1;
    private const int Int32Type = 0xd2;
    private const int Int64Type = 0xd3;


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
        switch (buffer[offset])
        {
            case BoolFalseType:
                return UnpackFalseExact();
            case BoolTrueType:
                return UnpackTrueExact();
            default:
                throw new MessageTypeException();
        }
    }

    private bool UnpackFalseExact()
    {
        Advance(1);
        return false;
    }

    private bool UnpackTrueExact()
    {
        Advance(1);
        return true;
    }

    internal object UnpackNull()
    {
        More(1);
        if (buffer[offset] != NilType)
        {
            throw new MessageTypeException();
        }
        return UnpackNullExact();
    }

    private object UnpackNullExact()
    {
        Advance(1);
        return null;
    }

    internal bool TryUnpackNull()
    {
        if (!TryMore(1))
        {
            return false;
        }
        if (buffer[offset] != NilType)
        {
            return false;
        }
        UnpackNullExact();
        return true;
    }

    internal float UnpackFloat()
    {
        var d = UnpackDouble();
        if (d < float.MinValue || d > float.MaxValue)
        {
            throw new MessagePackOverflowException("float");
        }
        return (float) d;
    }

    internal double UnpackDouble()
    {
        More(1);
        switch (buffer[offset])
        {
            case FloatType:
                return UnpackFloatExact();
            case DoubleType:
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
        if (IsFixRawType(b))  
        {
            return UnpackFixRawLengthExact(b);
        }
        switch (b)
        {
            case Raw16Type:
                return UnpackUInt16Exact();
            case Raw32Type:
                var v = UnpackUInt32Exact();
                if (v > int.MaxValue)
                {
                    throw new MessagePackOverflowException("int");
                }
                return (int) v;
            default:
                throw new MessageTypeException();
        }
    }

    private int UnpackFixRawLengthExact(byte b)
    {
        Advance(1);
        return b & 0x1f;
    }

    private static bool IsFixRawType(byte b)
    {
        return (b & 0xe0) == 0xa0;
    }

    internal byte[] UnpackRawBody(int length)
    {
        More(length);
        var bytes = new byte[length];
        Array.Copy(buffer, offset, bytes, 0, length);
        Advance(length);
        return bytes;
    }

    internal ulong UnpackULong()
    {
        More(1);
        byte b = buffer[offset];
        if (TryUnpackPositiveFixnum(b))
        {
            return b;
        }
        switch (b)
        {
            case UInt8Type:
                return UnpackUInt8Exact();
            case UInt16Type:
                return UnpackUInt16Exact();
            case UInt32Type: 
                return UnpackUInt32Exact();
            case UInt64Type: 
                return UnpackUInt64Exact();
            default:
                throw new MessageTypeException();
        }
    }

    internal long UnpackLong()
    {
        More(1);
        byte b = buffer[offset];
        if (TryUnpackPositiveFixnum(b))
        {
            return b;
        }
        if (TryUnpackNegativeFixnum(b))
        {
            return (sbyte)b;
        }
        switch (b)
        {
            case UInt8Type:
                return UnpackUInt8Exact();
            case UInt16Type:
                return UnpackUInt16Exact();
            case UInt32Type:
                return UnpackUInt32Exact();
            case UInt64Type:
                var v = UnpackUInt64Exact();
                if (v > long.MaxValue)
                {
                    throw new MessagePackOverflowException("long");
                }
                return (long)v;
            case Int8Type:
                return UnpackInt8Exact();
            case Int16Type: 
                return UnpackInt16Exact();
            case Int32Type:
                return UnpackInt32Exact();
            case Int64Type:
                return UnpackInt64Exact();
            default:
                throw new MessageTypeException();
        }
    }

    internal uint UnpackUInt()
    {
        More(1);
        byte b = buffer[offset];
        if (TryUnpackPositiveFixnum(b))
        {
            return b;
        }
        switch (b)
        {
            case UInt8Type:
                return UnpackUInt8Exact();
            case UInt16Type:
                return UnpackUInt16Exact();
            case UInt32Type:
                return UnpackUInt32Exact();
            default:
                throw new MessageTypeException();
        }
    }

    internal int UnpackInt()
    {
        More(1);
        byte b = buffer[offset];
        if (TryUnpackPositiveFixnum(b))
        {
            return b;
        }
        if (TryUnpackNegativeFixnum(b))
        {
            return (sbyte)b;
        }
        switch (b)
        {
            case UInt8Type:
                return UnpackUInt8Exact();
            case UInt16Type:
                return UnpackUInt16Exact();
            case UInt32Type:
                var v = UnpackUInt32Exact();
                if (v > int.MaxValue)
                {
                    throw new MessagePackOverflowException("int");
                }
                return (int)v;
            case Int8Type:
                return UnpackInt8Exact();
            case Int16Type:
                return UnpackInt16Exact();
            case Int32Type:
                return UnpackInt32Exact();
            default:
                throw new MessageTypeException();
        }
    }

    internal ushort UnpackUShort()
    {
        More(1);
        byte b = buffer[offset];
        if (TryUnpackPositiveFixnum(b))
        {
            return b;
        }
        switch (b)
        {
            case UInt8Type:
                return UnpackUInt8Exact();
            case UInt16Type:
                return UnpackUInt16Exact();
            default:
                throw new MessageTypeException();
        }
    }

    internal short UnpackShort()
    {
        More(1);
        byte b = buffer[offset];
        if (TryUnpackPositiveFixnum(b))
        {
            return b;
        }
        if (TryUnpackNegativeFixnum(b))
        {
            return (sbyte)b;
        }
        switch (b)
        {
            case UInt8Type:
                return UnpackUInt8Exact();
            case UInt16Type:
                var v = UnpackUInt16Exact();
                if (v > short.MaxValue)
                {
                    throw new MessagePackOverflowException("short");
                }
                return (short)v;
            case Int8Type:
                return UnpackInt8Exact();
            case Int16Type:
                return UnpackInt16Exact();
            default:
                throw new MessageTypeException();
        }
    }

    internal byte UnpackByte()
    {
        More(1);
        byte b = buffer[offset];
        if (TryUnpackPositiveFixnum(b))
        {
            return b;
        }
        switch (b)
        {
            case UInt8Type:
                return UnpackUInt8Exact();
            default:
                throw new MessageTypeException();
        }
    }

    internal sbyte UnpackSByte()
    {
        More(1);
        byte b = buffer[offset];
        if (TryUnpackPositiveFixnum(b))
        {
            return (sbyte)b;
        }
        if (TryUnpackNegativeFixnum(b))
        {
            return (sbyte)b;
        }
        switch (b)
        {
            case UInt8Type:
                var v = UnpackUInt8Exact();
                if (v > sbyte.MaxValue)
                {
                    throw new MessagePackOverflowException("sbyte");
                }
                return (sbyte)v;
            case Int8Type:
                return UnpackInt8Exact();
            default:
                throw new MessageTypeException();
        }
    }

    internal bool TryUnpackObject(out object result)
    {
        result = null;
        if (!TryMore(1))
        {
            return false;
        }
        byte b = buffer[offset];

        if (TryUnpackPositiveFixnum(b))
        {
            result = b;
        }
        else if (TryUnpackNegativeFixnum(b))
        {
            result = (sbyte)b;
        }
        else switch (b)
        {
            case NilType:
                result = UnpackNullExact();
                break;
            case BoolTrueType:
                result = UnpackTrueExact();
                break;
            case BoolFalseType:
                result = UnpackFalseExact();
                break;
            case DoubleType:
                result = UnpackDoubleExact();
                break;
            case FloatType:
                result = UnpackFloatExact();
                break;
            case UInt64Type:
                result = UnpackUInt64Exact();
                break;
            case Int64Type:
                result = UnpackInt64Exact();
                break;
            case UInt32Type:
                result = UnpackUInt32Exact();
                break;
            case Int32Type:
                result = UnpackInt32Exact();
                break;
            case UInt16Type:
                result = UnpackUInt16Exact();
                break;
            case Int16Type:
                result = UnpackInt16Exact();
                break;
            case UInt8Type:
                result = UnpackUInt8Exact();
                break;
            case Int8Type:
                result = UnpackInt8Exact();
                break;

            default:
                throw new MessageTypeException();
        }
        return true;
    }

    private bool TryUnpackPositiveFixnum(byte b)
    {
        if (IsPositiveFixnum(b))
        {
            Advance(1);
            return true;
        }
        return false;
    }

    private static bool IsPositiveFixnum(byte b)
    {
        return (b & 0x80) == 0;
    }

    private bool TryUnpackNegativeFixnum(byte b)
    {
        if (IsNegativeFixnum(b))
        {
            Advance(1);
            return true;
        }
        return false;
    }

    private static bool IsNegativeFixnum(byte b)
    {
        return (b & 0xe0) == 0xe0;
    }

    private sbyte UnpackInt8Exact()
    {
        More(2);
        Advance(2);
        return (sbyte) buffer[offset - 1];
    }

    private short UnpackInt16Exact()
    {
        More(3);
        Advance(3);
        return BitConverter.ToInt16(buffer, offset - 2);
    }

    private int UnpackInt32Exact()
    {
        More(5);
        Advance(5);
        return BitConverter.ToInt32(buffer, offset - 4);
    }

    private long UnpackInt64Exact()
    {
        More(9);
        Advance(9);
        return BitConverter.ToInt64(buffer, offset - 8);
    }

    private byte UnpackUInt8Exact()
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
            throw new UnpackException("Insufficient buffer");
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