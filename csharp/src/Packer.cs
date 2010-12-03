using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Packer
{
    private readonly BinaryWriterBigEndian writer;

    private Dictionary<Type, Action<object>> packerCallbacks;

    public Packer(Stream stream)
    {
        writer = new BinaryWriterBigEndian(stream);
        InitPackerCallbacks();
    }

    private void InitPackerCallbacks()
    {
        packerCallbacks = new Dictionary<Type, Action<object>>();

        packerCallbacks[typeof(bool)] = val => PackBool((bool) val);
        packerCallbacks[typeof(double)] = val => PackDouble((double) val);
        packerCallbacks[typeof(float)] = val => PackFloat((float) val);
        packerCallbacks[typeof(ulong)] = val => PackULong((ulong) val);
        packerCallbacks[typeof(long)] = val => PackLong((long) val);
        packerCallbacks[typeof(uint)] = val => PackUInt((uint) val);
        packerCallbacks[typeof(int)] = val => PackInt((int) val);
        packerCallbacks[typeof(ushort)] = val => PackUShort((ushort) val);
        packerCallbacks[typeof(short)] = val => PackShort((short) val);
        packerCallbacks[typeof(byte)] = val => PackByte((byte) val);
        packerCallbacks[typeof(sbyte)] = val => PackSByte((sbyte) val);
        packerCallbacks[typeof(char)] = val => PackChar((char) val);
        packerCallbacks[typeof(string)] = val => PackString((string) val);
        packerCallbacks[typeof(byte[])] = val => PackBytes((byte[]) val);
    }

    public Packer PackTrue()
    {
        writer.Write(MsgPack.BoolTrueType);
        return this;
    }

    public Packer PackFalse()
    {
        writer.Write(MsgPack.BoolFalseType);
        return this;
    }

    public Packer PackBool(bool d)
    {
        return d ? PackTrue() : PackFalse();
    }

    public Packer PackNull()
    {
        writer.Write(MsgPack.NilType);
        return this;
    }

    public Packer PackFloat(float f)
    {
        writer.Write(MsgPack.FloatType);
        writer.Write(f);
        return this;
    }

    public Packer PackDouble(double d)
    {
        writer.Write(MsgPack.DoubleType);
        writer.Write(d);
        return this;
    }

    public Packer PackString(string s)
    {
        if (s == null)
        {
            return PackNull();
        }
        return PackBytes(Encoding.UTF8.GetBytes(s));
    }

    public Packer PackRaw(int n)
    {
        if (n <= MsgPack.MaxFixRawLength)
        {
            writer.Write(MsgPack.PackFixRawLength(n));
        }
        else if (n <= MsgPack.MaxRaw16Length)
        {
            writer.Write(MsgPack.Raw16Type);
            writer.Write((ushort) n);
        }
        else
        {
            writer.Write(MsgPack.Raw32Type);
            writer.Write(n);
        }
        return this;
    }

    public Packer PackRawBody(byte[] b)
    {
        writer.Write(b);
        return this;
    }

    public Packer PackRawBody(byte[] b, int off, int length)
    {
        writer.Write(b, off, length);
        return this;
    }

    public Packer PackBytes(byte[] bytes)
    {
        if (bytes == null)
        {
            PackNull();
        }
        else
        {
            PackRaw(bytes.Length);
            PackRawBody(bytes);
        }
        return this;
    }

    public Packer PackULong(ulong d)
    {
        if (d <= MsgPack.MaxFixnum) 
        {
            PackFixnumExact((sbyte)d);
        }
        else
        {
            if (d <= MsgPack.MaxUInt16)
            {
                if (d <= MsgPack.MaxUInt8)
                {
                    PackUInt8Exact((byte)d);
                }
                else
                {
                    PackUInt16Exact((ushort)d);
                }
            }
            else
            {
                if (d <= MsgPack.MaxUInt32)
                {
                    PackUInt32Exact((uint)d);
                }
                else
                {
                    PackUInt64Exact(d);
                }
            }
        }

        return this;
    }

    public Packer PackLong(long d)
    {
        if (d > 0)
        {
            PackULong((ulong)d);
        }
        else if (d >= MsgPack.MinFixnum)
        {
            PackFixnumExact((sbyte) d);
        }
        else
        {
            if (d < -(1L << 15))
            {
                if (d < MsgPack.MinInt32)
                {
                    PackInt64Exact(d);
                }
                else
                {
                    PackInt32Exact((int) d);
                }
            }
            else
            {
                if (d < MsgPack.MinInt8)
                {
                    PackInt16Exact((short) d);
                }
                else
                {
                    PackInt8Exact((sbyte) d);
                }
            }
        }
        return this;
    }

    public Packer PackUInt(uint d)
    {
        if (d <= MsgPack.MaxFixnum)
        {
            PackFixnumExact((sbyte)d);
        }
        else
        {
            if (d <= MsgPack.MaxUInt16)
            {
                if (d <= MsgPack.MaxUInt8)
                {
                    PackUInt8Exact((byte)d);
                }
                else
                {
                    PackUInt16Exact((ushort)d);
                }
            }
            else
            {
                PackUInt32Exact(d);
            }
        }

        return this;
    }

    public Packer PackInt(int d)
    {
        if (d > 0)
        {
            PackUInt((uint)d);
        }
        else if (d >= MsgPack.MinFixnum)
        {
            PackFixnumExact((sbyte)d);
        }
        else
        {
            if (d < -(1L << 15))
            {
                PackInt32Exact(d);
            }
            else
            {
                if (d < MsgPack.MinInt8)
                {
                    PackInt16Exact((short)d);
                }
                else
                {
                    PackInt8Exact((sbyte)d);
                }
            }
        }
        return this;
    }

    public Packer PackUShort(ushort d)
    {
        if (d <= MsgPack.MaxFixnum)
        {
            PackFixnumExact((sbyte)d);
        }
        else
        {
            if (d <= MsgPack.MaxUInt8)
            {
                PackUInt8Exact((byte)d);
            }
            else
            {
                PackUInt16Exact(d);
            }
        }

        return this;
    }

    public Packer PackShort(short d)
    {
        if (d > 0)
        {
            PackUShort((ushort)d);
        }
        else if (d >= MsgPack.MinFixnum)
        {
            PackFixnumExact((sbyte)d);
        }
        else
        {
            if (d < MsgPack.MinInt8)
            {
                PackInt16Exact(d);
            }
            else
            {
                PackInt8Exact((sbyte)d);
            }
        }
        return this;
    }

    public Packer PackByte(byte d)
    {
        if (d <= MsgPack.MaxFixnum)
        {
            PackFixnumExact((sbyte)d);
        }
        else
        {
            PackUInt8Exact(d);
        }

        return this;
    }

    public Packer PackSByte(sbyte d)
    {
        if (MsgPack.MinFixnum <= d && d <= MsgPack.MaxFixnum)
        {
            PackFixnumExact(d);
        }
        else
        {
            PackInt8Exact(d);
        }
        return this;
    }

    public Packer PackChar(char ch)
    {
        return PackInt(ch);
    }

    public Packer PackEnum<T>(T e)
    {
        return PackInt(Convert.ToInt32(e));
    }

    public Packer Pack<TValue>(TValue val) where TValue : class, IMessagePackable
    {
        if (val == null)
        {
            PackNull();
        }
        else
        {
            val.ToMsgPack(this);
        }
        return this;
    }

    public Packer PackObject(object val)
    {
        PackObject(val, null);
        return this;
    }

    private void PackObject(object val, Action<object> packerCallback)
    {
        if (val == null)
        {
            PackNull();
        }
        else
        {
            if (packerCallback == null)
            {
                packerCallback = FindPackerCallback(val.GetType());
            }
            packerCallback(val);
        }
    }

    private Action<object> FindPackerCallback(Type type)
    {
        Action<object> callback;
        if (!TryFindPackerCallback(type, out callback))
        {
            throw new MessagePackException(string.Format("Cannot pack object of type {0}", type.FullName));
        }
        return callback;
    }

    private bool TryFindPackerCallback(Type type, out Action<object> callback)
    {
        if (type.IsEnum)
        {
            callback = v => PackEnum(v);
        }
        else if (typeof(IMessagePackable).IsAssignableFrom(type))
        {
            callback = v => Pack((IMessagePackable)v);
        }
        else if (typeof(IDictionary).IsAssignableFrom(type))
        {
            callback = v => PackDictionary((IDictionary)v);
        }
        else if (typeof(ICollection).IsAssignableFrom(type))
        {
            callback = v => PackCollection((ICollection)v);
        }
        else
        {
            packerCallbacks.TryGetValue(type, out callback);
        }
        return callback != null;
    }

    private Packer PackCollection(ICollection values)
    {
        return values == null ? PackNull() : PackCollection(values, values.Count, null);
    }

    public Packer PackCollection<T>(ICollection<T> values)
    {
        return values == null ? PackNull() : PackCollection(values, values.Count, FindPackerCallback(typeof (T)));
    }

    private Packer PackCollection(IEnumerable values, int count, Action<object> packerCallback)
    {
        if (values == null)
        {
            return PackNull();
        }
        PackArray(count);
        foreach (var value in values)
        {
            PackObject(value, packerCallback);
        }
        return this;
    }

    public Packer PackDictionary(IDictionary dict)
    {
        if (dict == null)
        {
            return PackNull();
        }
        PackMap(dict.Count);
        foreach (var key in dict.Keys)
        {
            PackObject(key);
            PackObject(dict[key]);
        }
        return this;
    }

    private void PackArray(int n)
    {
        if (n <= MsgPack.MaxFixArrayLength)
        {
            writer.Write(MsgPack.PackFixArayLength(n));
        }
        else if (n <= MsgPack.MaxArray16Length)
        {
            writer.Write(MsgPack.Array16Type);
            writer.Write((ushort) n);
        }
        else
        {
            writer.Write(MsgPack.Array32Type);
            writer.Write(n);
        }
    }

    private void PackMap(int n)
    {
        if (n <= MsgPack.MaxFixMapLength)
        {
            writer.Write(MsgPack.PackFixMapLength(n));
        }
        else if (n <= MsgPack.MaxMap16Length)
        {
            writer.Write(MsgPack.Map16Type);
            writer.Write((ushort)n);
        }
        else
        {
            writer.Write(MsgPack.Map32Type);
            writer.Write(n);
        }
    }

    private void PackInt8Exact(sbyte d)
    {
        writer.Write(MsgPack.Int8Type);
        writer.Write(d);
    }

    private void PackInt16Exact(short d)
    {
        writer.Write(MsgPack.Int16Type);
        writer.Write(d);
    }

    private void PackInt32Exact(int d)
    {
        writer.Write(MsgPack.Int32Type);
        writer.Write(d);
    }

    private void PackInt64Exact(long d)
    {
        writer.Write(MsgPack.Int64Type);
        writer.Write(d);
    }

    private void PackFixnumExact(sbyte d)
    {
        writer.Write(d);
    }

    private void PackUInt8Exact(byte d)
    {
        writer.Write(MsgPack.UInt8Type);
        writer.Write(d);
    }

    private void PackUInt16Exact(ushort d)
    {
        writer.Write(MsgPack.UInt16Type);writer.Write(d);
    }

    private void PackUInt32Exact(uint d)
    {
        writer.Write(MsgPack.UInt32Type);
        writer.Write(d);
    }

    private void PackUInt64Exact(ulong d)
    {
        writer.Write(MsgPack.UInt64Type);
        writer.Write(d);
    }
}

class MyBinaryWriter: BinaryWriter
{
    public override void Write(long l)
    {
        
    }
}