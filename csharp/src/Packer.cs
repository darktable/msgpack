using System.IO;

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
        writer.Write((byte)0xc3);
		return this;
	}

	public Packer PackFalse()  
    {
        writer.Write((byte)0xc2);
		return this;
	}

	public Packer PackBool(bool d)  
    {
		return d ? PackTrue() : PackFalse();
	}

    public Packer PackNull() 
    {
        writer.Write((byte)0xc0);
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
}
