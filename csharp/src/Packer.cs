using System.IO;

public class Packer
{
    private readonly Stream outputStream;

    public Packer(Stream outputStream)
    {
        this.outputStream = outputStream;
    }

/*    public Packer Pack(int val)
    {
        return PackInt(val);
    }

    public Packer PackInt(int d)
    {
        return this;
    }*/

    public Packer Pack(bool val)
    {
        return PackBool(val);
    }

	public Packer PackTrue()  
    {
        outputStream.WriteByte(0xc3);
		return this;
	}

	public Packer PackFalse()  
    {
        outputStream.WriteByte(0xc2);
		return this;
	}

	public Packer PackBool(bool d)  
    {
		return d ? PackTrue() : PackFalse();
	}
}
