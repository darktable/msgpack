using System;
using System.IO;
using System.Text;

public class Packer
{
    private const uint MaxPositiveFixnum = (1 << 7) - 1;
    private const uint MaxUInt8 = (1 << 8) - 1;
    private const ulong MaxUInt16 = (1L << 16) - 1;
    private const ulong MaxUInt32 = (1L << 32) - 1;
    private const int MaxFixRawLength = 31;
    private const int MaxRaw16Length = 65535;

    private readonly BinaryWriter writer;

    public Packer(Stream stream)
    {
        writer = new BinaryWriter(stream);
    }

    public Packer Pack(bool val)
    {
        return PackBool(val);
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

        byte[] b = Encoding.UTF8.GetBytes(s);
        return PackRaw(b.Length).PackRawBody(b);
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

    public Packer PackULong(ulong d)
    {
        if (d <= MaxPositiveFixnum) 
        {
            PackPositiveFixnumExact(d);
        }
        else
        {
            if (d <= MaxUInt16)
            {
                if (d <= MaxUInt8)
                {
                    PackUInt8Exact(d);
                }
                else
                {
                    PackUInt16Exact(d);
                }
            }
            else
            {
                if (d <= MaxUInt32)
                {
                    PackUInt32Exact(d);
                }
                else
                {
                    PackUInt64Exact(d);
                }
            }
        }

        return this;
    }

    private void PackPositiveFixnumExact(ulong d)
    {
        writer.Write((byte) d);
    }

    private void PackUInt8Exact(ulong d)
    {
        writer.Write((byte)0xcc);
        writer.Write((byte)d);
    }

    private void PackUInt16Exact(ulong d)
    {
        writer.Write((byte)0xcd);
        writer.Write((UInt16)d);
    }

    private void PackUInt32Exact(ulong d)
    {
        writer.Write((byte)0xce);
        writer.Write((UInt32)d);
    }

    private void PackUInt64Exact(ulong d)
    {
        writer.Write((byte)0xcf);
        writer.Write(d);
    }
}