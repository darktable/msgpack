using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Unpacker
{
    private readonly BinaryReader reader;

    public Unpacker(Stream stream)
    {
        reader = new BinaryReader(stream);
    }

    public bool UnpackBool()
    {
        switch (reader.ReadByte())
        {
            case MsgPack.BoolFalseType:
                return false;
            case MsgPack.BoolTrueType:
                return true;
            default:
                throw new MessageTypeException();
        }
    }

    public object UnpackNull()
    {
        if (reader.ReadByte() != MsgPack.NilType)
        {
            throw new MessageTypeException();
        }
        return null;
    }

    public float UnpackFloat()
    {
        var d = UnpackDouble();
        if ((d < float.MinValue || d > float.MaxValue) && !double.IsInfinity(d))
        {
            throw new MessagePackOverflowException("float");
        }
        return (float)d;
    }

    public double UnpackDouble()
    {
        switch (reader.ReadByte())
        {
            case MsgPack.FloatType:
                return reader.ReadSingle();
            case MsgPack.DoubleType:
                return reader.ReadDouble();
            default:
                throw new MessageTypeException();
        }
    }

    public string UnpackString()
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
        return UnpackRawBody(length);
    }

    public ulong UnpackULong()
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsPositiveFixnum(b))
        {
            return b;
        }
        switch (b)
        {
            case MsgPack.UInt8Type:
                return reader.ReadByte();
            case MsgPack.UInt16Type:
                return reader.ReadUInt16();
            case MsgPack.UInt32Type:
                return reader.ReadUInt32();
            case MsgPack.UInt64Type:
                return reader.ReadUInt64();
            default:
                throw new MessageTypeException();
        }
    }

    public long UnpackLong()
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsPositiveFixnum(b))
        {
            return b;
        }
        if (MsgPack.IsNegativeFixnum(b))
        {
            return (sbyte)b;
        }
        switch (b)
        {
            case MsgPack.UInt8Type:
                return reader.ReadByte();
            case MsgPack.UInt16Type:
                return reader.ReadUInt16();
            case MsgPack.UInt32Type:
                return reader.ReadUInt32();
            case MsgPack.UInt64Type:
                var v = reader.ReadUInt64();
                if (v > long.MaxValue)
                {
                    throw new MessagePackOverflowException("long");
                }
                return (long)v;
            case MsgPack.Int8Type:
                return (sbyte)reader.ReadByte();
            case MsgPack.Int16Type:
                return reader.ReadInt16();
            case MsgPack.Int32Type:
                return reader.ReadInt32();
            case MsgPack.Int64Type:
                return reader.ReadInt64();
            default:
                throw new MessageTypeException();
        }
    }

    public uint UnpackUInt()
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsPositiveFixnum(b))
        {
            return b;
        }
        switch (b)
        {
            case MsgPack.UInt8Type:
                return reader.ReadByte();
            case MsgPack.UInt16Type:
                return reader.ReadUInt16();
            case MsgPack.UInt32Type:
                return reader.ReadUInt32();
            default:
                throw new MessageTypeException();
        }
    }

    public int UnpackInt()
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsPositiveFixnum(b))
        {
            return b;
        }
        if (MsgPack.IsNegativeFixnum(b))
        {
            return (sbyte)b;
        }
        switch (b)
        {
            case MsgPack.UInt8Type:
                return reader.ReadByte();
            case MsgPack.UInt16Type:
                return reader.ReadUInt16();
            case MsgPack.UInt32Type:
                var v = reader.ReadUInt32();
                if (v > int.MaxValue)
                {
                    throw new MessagePackOverflowException("int");
                }
                return (int)v;
            case MsgPack.Int8Type:
                return (sbyte)reader.ReadByte();
            case MsgPack.Int16Type:
                return reader.ReadInt16();
            case MsgPack.Int32Type:
                return reader.ReadInt32();
            default:
                throw new MessageTypeException();
        }
    }

    public ushort UnpackUShort()
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsPositiveFixnum(b))
        {
            return b;
        }
        switch (b)
        {
            case MsgPack.UInt8Type:
                return reader.ReadByte();
            case MsgPack.UInt16Type:
                return reader.ReadUInt16();
            default:
                throw new MessageTypeException();
        }
    }

    public short UnpackShort()
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsPositiveFixnum(b))
        {
            return b;
        }
        if (MsgPack.IsNegativeFixnum(b))
        {
            return (sbyte)b;
        }
        switch (b)
        {
            case MsgPack.UInt8Type:
                return reader.ReadByte();
            case MsgPack.UInt16Type:
                var v = reader.ReadUInt16();
                if (v > short.MaxValue)
                {
                    throw new MessagePackOverflowException("short");
                }
                return (short)v;
            case MsgPack.Int8Type:
                return (sbyte)reader.ReadByte();
            case MsgPack.Int16Type:
                return reader.ReadInt16();
            default:
                throw new MessageTypeException();
        }
    }

    public byte UnpackByte()
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsPositiveFixnum(b))
        {
            return b;
        }
        switch (b)
        {
            case MsgPack.UInt8Type:
                return reader.ReadByte();
            default:
                throw new MessageTypeException();
        }
    }

    public sbyte UnpackSByte()
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsPositiveFixnum(b))
        {
            return (sbyte)b;
        }
        if (MsgPack.IsNegativeFixnum(b))
        {
            return (sbyte)b;
        }
        switch (b)
        {
            case MsgPack.UInt8Type:
                var v = reader.ReadByte();
                if (v > sbyte.MaxValue)
                {
                    throw new MessagePackOverflowException("sbyte");
                }
                return (sbyte)v;
            case MsgPack.Int8Type:
                return (sbyte)reader.ReadByte();
            default:
                throw new MessageTypeException();
        }
    }

    public object UnpackObject()
    {
        int rawLength;
        int arrayLength;
        if (TryUnpackRaw(out rawLength))
        {
            return UnpackRawBody(rawLength);
        }
        if (TryUnpackArray(out arrayLength))
        {
            return UnpackObjectListBody(arrayLength);
        }

        byte b = reader.ReadByte();
        if (MsgPack.IsPositiveFixnum(b))
        {
            return b;
        }
        if (MsgPack.IsNegativeFixnum(b))
        {
            return (sbyte)b;
        }
        switch (b)
        {
            case MsgPack.NilType:
                return null;
            case MsgPack.BoolTrueType:
                return true;
            case MsgPack.BoolFalseType:
                return false;
            case MsgPack.DoubleType:
                return reader.ReadDouble();
            case MsgPack.FloatType:
                return reader.ReadSingle();
            case MsgPack.UInt64Type:
                return reader.ReadUInt64();
            case MsgPack.Int64Type:
                return reader.ReadInt64();
            case MsgPack.UInt32Type:
                return reader.ReadUInt32();
            case MsgPack.Int32Type:
                return reader.ReadInt32();
            case MsgPack.UInt16Type:
                return reader.ReadUInt16();
            case MsgPack.Int16Type:
                return reader.ReadInt16();
            case MsgPack.UInt8Type:
                return reader.ReadByte();
            case MsgPack.Int8Type:
                return (sbyte)reader.ReadByte();
            default:
                throw new MessageTypeException();
        }
    }

    public int UnpackArray()
    {
        int length;
        if (TryUnpackArray(out length))
        {
            return length;
        }
        throw new MessageTypeException();
    }

    public int UnpackMap()
    {
        int length;
        if (TryUnpackMap(out length))
        {
            return length;
        }
        throw new MessageTypeException();
    }

    public TValue Unpack<TValue>() where TValue : class, IMessagePackable, new()
    {
        if (TryUnpackNull())
        {
            return null;
        }
        var val = new TValue();
        val.FromMsgPack(this);
        return val;
    }

    public char UnpackChar()
    {
        return (char)UnpackInt();
    }

    public T UnpackEnum<T>()
    {
        return (T)Enum.ToObject(typeof(T), UnpackInt());
    }

    public List<object> UnpackObjectList()
    {
        if (TryUnpackNull())
        {
            return null;
        }
        var length = UnpackArray();
        return UnpackObjectListBody(length);
    }

    public List<T> UnpackList<T>() where T : class, IMessagePackable, new()
    {
        if (TryUnpackNull())
        {
            return null;
        }
        var length = UnpackArray();
        var list = new List<T>(length);
        for (int i = 0; i < length; i++)
        {
            list.Add(Unpack<T>());
        }
        return list;
    }

    public Dictionary<object, object> UnpackDictionary()
    {
        if (TryUnpackNull())
        {
            return null;
        }
        var length = UnpackMap();
        var dict = new Dictionary<object, object>();
        for (int i = 0; i < length; i++)
        {
            dict.Add(UnpackObject(), UnpackObject());
        }
        return dict;
    }

    private int UnpackRaw()
    {
        int length;
        if (TryUnpackRaw(out length))
        {
            return length;
        }
        throw new MessageTypeException();
    }

    private bool TryUnpackRaw(out int length)
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsFixRawType(b))
        {
            length = MsgPack.UnpackFixRawLength(b);
        }
        else switch (b)
        {
            case MsgPack.Raw16Type:
                length = reader.ReadUInt16();
                break;
            case MsgPack.Raw32Type:
                var v = reader.ReadUInt32();
                if (v > int.MaxValue)
                {
                    throw new MessagePackOverflowException("int");
                }
                length = (int)v;
                break;
            default:
                GotoBack();
                length = 0;
                return false;
        }
        return true;
    }

    private byte[] UnpackRawBody(int length)
    {
        var bytes = reader.ReadBytes(length);
        if (bytes.Length < length)
        {
            throw new EndOfStreamException();
        }
        return bytes;
    }

    private bool TryUnpackArray(out int length)
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsFixArrayType(b))
        {
            length = MsgPack.UnpackFixArrayLength(b);
        }
        else switch (b)
        {
            case MsgPack.Array16Type:
                length = reader.ReadUInt16();
                break;
            case MsgPack.Array32Type:
                var v = reader.ReadUInt32();
                if (v > int.MaxValue)
                {
                    throw new MessagePackOverflowException("int");
                }
                length = (int)v;
                break;
            default:
                GotoBack();
                length = 0;
                return false;
        }
        return true;
    }

    private bool TryUnpackMap(out int length)
    {
        byte b = reader.ReadByte();
        if (MsgPack.IsFixMapType(b))
        {
            length = MsgPack.UnpackFixMapLength(b);
        }
        else switch (b)
        {
            case MsgPack.Map16Type:
                length = reader.ReadUInt16();
                break;
            case MsgPack.Map32Type:
                var v = reader.ReadUInt32();
                if (v > int.MaxValue)
                {
                    throw new MessagePackOverflowException("int");
                }
                length = (int)v;
                break;
            default:
                GotoBack();
                length = 0;
                return false;
        }
        return true;
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

    private bool TryUnpackNull()
    {
        if (reader.ReadByte() != MsgPack.NilType)
        {
            GotoBack();
            return false;
        }
        return true;
    }

    private void GotoBack()
    {
        reader.BaseStream.Seek(-1, SeekOrigin.Current);
    }
}
