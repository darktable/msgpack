using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Packer
{
    private const byte MaxFixnum = (1 << 7) - 1;
    private const sbyte MinFixnum = -(1 << 5);
    private const uint MaxUInt8 = (1 << 8) - 1;
    private const ulong MaxUInt16 = (1L << 16) - 1;
    private const ulong MaxUInt32 = (1L << 32) - 1;
    private const int MaxFixRawLength = 31;
    private const int MaxRaw16Length = 65535;
    private const long MinInt32 = -(1L<<31);
    private const int MinInt8 = -(1<<7);

    private readonly BinaryWriter writer;

    private Dictionary<Type, Action<object>> packerCallbacks;

    public Packer(Stream stream)
    {
        writer = new BinaryWriter(stream);
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
        writer.Write((byte) 0xc3);
        return this;
    }

    public Packer PackFalse()
    {
        writer.Write((byte) 0xc2);
        return this;
    }

    public Packer PackBool(bool d)
    {
        return d ? PackTrue() : PackFalse();
    }

    public Packer PackNull()
    {
        writer.Write((byte) 0xc0);
        return this;
    }

    public Packer PackFloat(float f)
    {
        writer.Write((byte) 0xca);
        writer.Write(f);
        return this;
    }

    public Packer PackDouble(double d)
    {
        writer.Write((byte) 0xcb);
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
        if (n <= MaxFixRawLength)
        {
            var b = (byte) (0xa0 | n);
            writer.Write(b);
        }
        else if (n <= MaxRaw16Length)
        {
            writer.Write((byte) 0xda);
            writer.Write((ushort) n);
        }
        else
        {
            writer.Write((byte) 0xdb);
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
        if (d <= MaxFixnum) 
        {
            PackFixnumExact((sbyte)d);
        }
        else
        {
            if (d <= MaxUInt16)
            {
                if (d <= MaxUInt8)
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
                if (d <= MaxUInt32)
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
        else if (d >= MinFixnum)
        {
            PackFixnumExact((sbyte) d);
        }
        else
        {
            if (d < -(1L << 15))
            {
                if (d < MinInt32)
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
                if (d < MinInt8)
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
        if (d <= MaxFixnum)
        {
            PackFixnumExact((sbyte)d);
        }
        else
        {
            if (d <= MaxUInt16)
            {
                if (d <= MaxUInt8)
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
        else if (d >= MinFixnum)
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
                if (d < MinInt8)
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
        if (d <= MaxFixnum)
        {
            PackFixnumExact((sbyte)d);
        }
        else
        {
            if (d <= MaxUInt8)
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
        else if (d >= MinFixnum)
        {
            PackFixnumExact((sbyte)d);
        }
        else
        {
            if (d < MinInt8)
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
        if (d <= MaxFixnum)
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
        if (MinFixnum <= d && d <= MaxFixnum)
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
        if (val == null)
        {
            PackNull();
        }
        else
        {
            var type = val.GetType();
            Action<object> packerCallback;
            if (type.IsEnum)
            {
                packerCallback = v=> PackEnum(v);
            }
            else if (typeof(IMessagePackable).IsInstanceOfType(val))
            {
                packerCallback = v => Pack((IMessagePackable)v);
            }
            else
            {
                packerCallbacks.TryGetValue(type, out packerCallback);
            }

            if (packerCallback != null)
            {
                packerCallback(val);
            }
            else
            {
                throw new MessagePackException(string.Format("Cannot pack object of type {0}", type.FullName));
            }
        }
        return this;
    }

    private void PackInt8Exact(sbyte d)
    {
        writer.Write((byte)0xd0);
        writer.Write(d);
    }

    private void PackInt16Exact(short d)
    {
        writer.Write((byte)0xd1);
        writer.Write(d);
    }

    private void PackInt32Exact(int d)
    {
        writer.Write((byte)0xd2);
        writer.Write(d);
    }

    private void PackInt64Exact(long d)
    {
        writer.Write((byte)0xd3);
        writer.Write(d);
    }

    private void PackFixnumExact(sbyte d)
    {
        writer.Write(d);
    }

    private void PackUInt8Exact(byte d)
    {
        writer.Write((byte)0xcc);
        writer.Write(d);
    }

    private void PackUInt16Exact(ushort d)
    {
        writer.Write((byte)0xcd);
        writer.Write(d);
    }

    private void PackUInt32Exact(uint d)
    {
        writer.Write((byte)0xce);
        writer.Write(d);
    }

    private void PackUInt64Exact(ulong d)
    {
        writer.Write((byte)0xcf);
        writer.Write(d);
    }
}