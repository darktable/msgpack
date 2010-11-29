using System.IO;
using System.Text;

public class Packer
{
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
        if (n < 32)
        {
            var b = (byte) (0xa0 | n);
            writer.Write(b);
        }
        else if (n < 65536)
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
}