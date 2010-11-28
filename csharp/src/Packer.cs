using System.IO;

public class Packer
{
    private readonly Stream stream;

    public Packer(Stream stream)
    {
        this.stream = stream;
    }

    public Packer Pack(bool val)
    {
        return PackBool(val);
    }

	public Packer PackTrue()  
    {
        stream.WriteByte(0xc3);
		return this;
	}

	public Packer PackFalse()  
    {
        stream.WriteByte(0xc2);
		return this;
	}

	public Packer PackBool(bool d)  
    {
		return d ? PackTrue() : PackFalse();
	}

    public Packer PackNull() 
    {
        stream.WriteByte(0xc0);
		return this;
	}
}
