using System;
using System.Collections.Generic;
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
        switch (buffer[offset])
        {
            case MsgPack.BoolFalseType:
                return UnpackFalseExact();
            case MsgPack.BoolTrueType:
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
        if (buffer[offset] != MsgPack.NilType)
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
        if (buffer[offset] != MsgPack.NilType)
        {
            return false;
        }
        UnpackNullExact();
        return true;
    }

    internal float UnpackFloat()
    {
        var d = UnpackDouble();
        if ((d < float.MinValue || d > float.MaxValue) && !double.IsInfinity(d))
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
            case MsgPack.FloatType:
                return UnpackFloatExact();
            case MsgPack.DoubleType:
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
        String s;
        try
        {
            s = Encoding.UTF8.GetString(UnpackBytes());
        }
        catch (Exception e)
        {
            throw new MessageTypeException(e);
        }
        return s;
    }

    public byte[] UnpackBytes()
    {
        if (TryUnpackNull())
        {
            return null;
        }
        int length = UnpackRaw();
        More(length);
        var result = new byte[length];
        Array.Copy(buffer, offset, result, 0, length);
        Advance(length);
        return result;
    }

    internal int UnpackRaw()
    {
        int length;
        if (TryUnpackRaw(out length))
        {
            return length;
        }
        throw new MessageTypeException();
    }

    internal bool TryUnpackRaw(out int length)
    {
        More(1);
        byte b = buffer[offset];
        if (MsgPack.IsFixRawType(b))  
        {
            length = UnpackFixRawLengthExact(b);
        } 
        else switch (b)
        {
            case MsgPack.Raw16Type:
                length = UnpackUInt16Exact();
                break;
            case MsgPack.Raw32Type:
                var v = UnpackUInt32Exact();
                if (v > int.MaxValue)
                {
                    throw new MessagePackOverflowException("int");
                }
                length = (int) v;
                break;
            default:
                length = 0;
                return false;
        }
        return true;
    }

    internal int UnpackArray()
    {
        int length;
        if (TryUnpackArray(out length))
        {
            return length;
        }
        throw new MessageTypeException();
    }

    internal bool TryUnpackArray(out int length)
    {
        More(1);
        byte b = buffer[offset];
        if (MsgPack.IsFixArrayType(b))
        {
            length = UnpackFixArrayLengthExact(b);
        }
        else switch (b)
        {
            case MsgPack.Array16Type:
                length = UnpackUInt16Exact();
                break;
            case MsgPack.Array32Type:
                var v = UnpackUInt32Exact();
                if (v > int.MaxValue)
                {
                    throw new MessagePackOverflowException("int");
                }
                length = (int)v;
                break;
            default:
                length = 0;
                return false;
        }
        return true;
    }

    private int UnpackFixArrayLengthExact(byte b)
    {
        Advance(1);
        return b & 0x0f;
    }

    private int UnpackFixRawLengthExact(byte b)
    {
        Advance(1);
        return b & 0x1f;
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
            case MsgPack.UInt8Type:
                return UnpackUInt8Exact();
            case MsgPack.UInt16Type:
                return UnpackUInt16Exact();
            case MsgPack.UInt32Type: 
                return UnpackUInt32Exact();
            case MsgPack.UInt64Type: 
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
            case MsgPack.UInt8Type:
                return UnpackUInt8Exact();
            case MsgPack.UInt16Type:
                return UnpackUInt16Exact();
            case MsgPack.UInt32Type:
                return UnpackUInt32Exact();
            case MsgPack.UInt64Type:
                var v = UnpackUInt64Exact();
                if (v > long.MaxValue)
                {
                    throw new MessagePackOverflowException("long");
                }
                return (long)v;
            case MsgPack.Int8Type:
                return UnpackInt8Exact();
            case MsgPack.Int16Type: 
                return UnpackInt16Exact();
            case MsgPack.Int32Type:
                return UnpackInt32Exact();
            case MsgPack.Int64Type:
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
            case MsgPack.UInt8Type:
                return UnpackUInt8Exact();
            case MsgPack.UInt16Type:
                return UnpackUInt16Exact();
            case MsgPack.UInt32Type:
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
            case MsgPack.UInt8Type:
                return UnpackUInt8Exact();
            case MsgPack.UInt16Type:
                return UnpackUInt16Exact();
            case MsgPack.UInt32Type:
                var v = UnpackUInt32Exact();
                if (v > int.MaxValue)
                {
                    throw new MessagePackOverflowException("int");
                }
                return (int)v;
            case MsgPack.Int8Type:
                return UnpackInt8Exact();
            case MsgPack.Int16Type:
                return UnpackInt16Exact();
            case MsgPack.Int32Type:
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
            case MsgPack.UInt8Type:
                return UnpackUInt8Exact();
            case MsgPack.UInt16Type:
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
            case MsgPack.UInt8Type:
                return UnpackUInt8Exact();
            case MsgPack.UInt16Type:
                var v = UnpackUInt16Exact();
                if (v > short.MaxValue)
                {
                    throw new MessagePackOverflowException("short");
                }
                return (short)v;
            case MsgPack.Int8Type:
                return UnpackInt8Exact();
            case MsgPack.Int16Type:
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
            case MsgPack.UInt8Type:
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
            case MsgPack.UInt8Type:
                var v = UnpackUInt8Exact();
                if (v > sbyte.MaxValue)
                {
                    throw new MessagePackOverflowException("sbyte");
                }
                return (sbyte)v;
            case MsgPack.Int8Type:
                return UnpackInt8Exact();
            default:
                throw new MessageTypeException();
        }
    }

    internal char UnpackChar()
    {
        return (char)UnpackInt();
    }

    internal T UnpackEnum<T>()
    {
        return (T)Enum.ToObject(typeof(T), UnpackInt());
    }

    internal TValue Unpack<TValue>(Unpacker unpacker) where TValue : class, IMessagePackable, new()
    {
        if (TryUnpackNull())
        {
            return null;
        }
        var val = new TValue();
        val.FromMsgPack(unpacker);
        return val;
    }

    internal bool TryUnpackObject(out object result)
    {
        result = null;
        if (!TryMore(1))
        {
            return false;
        }
        byte b = buffer[offset];
        int rawLength;
        int arrayLength;
        if (TryUnpackPositiveFixnum(b))
        {
            result = b;
        }
        else if (TryUnpackNegativeFixnum(b))
        {
            result = (sbyte)b;
        }
        else if (TryUnpackRaw(out rawLength))
        {
            result = UnpackRawBody(rawLength);
        }
        else if (TryUnpackArray(out arrayLength))
        {
            result = UnpackObjectListBody(arrayLength);
        }
        else switch (b)
        {
            case MsgPack.NilType:
                result = UnpackNullExact();
                break;
            case MsgPack.BoolTrueType:
                result = UnpackTrueExact();
                break;
            case MsgPack.BoolFalseType:
                result = UnpackFalseExact();
                break;
            case MsgPack.DoubleType:
                result = UnpackDoubleExact();
                break;
            case MsgPack.FloatType:
                result = UnpackFloatExact();
                break;
            case MsgPack.UInt64Type:
                result = UnpackUInt64Exact();
                break;
            case MsgPack.Int64Type:
                result = UnpackInt64Exact();
                break;
            case MsgPack.UInt32Type:
                result = UnpackUInt32Exact();
                break;
            case MsgPack.Int32Type:
                result = UnpackInt32Exact();
                break;
            case MsgPack.UInt16Type:
                result = UnpackUInt16Exact();
                break;
            case MsgPack.Int16Type:
                result = UnpackInt16Exact();
                break;
            case MsgPack.UInt8Type:
                result = UnpackUInt8Exact();
                break;
            case MsgPack.Int8Type:
                result = UnpackInt8Exact();
                break;

            default:
                throw new MessageTypeException();
        }
        return true;
    }

    internal object UnpackObject()
    {
        object result;
        if (!TryUnpackObject(out result))
        {
            throw new UnpackException("Not enough data in stream.");
        }
        return result;
    }

    internal List<object> UnpackObjectList()
    {
        var length = UnpackArray();
        return UnpackObjectListBody(length);
    }

    private List<object> UnpackObjectListBody(int length)
    {
        var list = new List<object>(length);
        for (int i = 0; i < length; i++)
        {
            list.Add(UnpackObject());
        }
        return list;
    }

    internal List<T> UnpackList<T>(Unpacker unpacker) where T : class, IMessagePackable, new()
    {
        var length = UnpackArray();
        var list = new List<T>(length);
        for (int i = 0; i < length; i++)
        {
            list.Add(Unpack<T>(unpacker));
        }
        return list;
    }

    private bool TryUnpackPositiveFixnum(byte b)
    {
        if (MsgPack.IsPositiveFixnum(b))
        {
            Advance(1);
            return true;
        }
        return false;
    }

    private bool TryUnpackNegativeFixnum(byte b)
    {
        if (MsgPack.IsNegativeFixnum(b))
        {
            Advance(1);
            return true;
        }
        return false;
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