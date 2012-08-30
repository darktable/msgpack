using System;
using System.IO;

namespace MsgPack {
	internal class BinaryWriterBigEndian : IDisposable
	{
	    private readonly byte[] buffer;
	    private readonly Stream stream;

	    public BinaryWriterBigEndian(Stream output)
	    {
	        if (output == null)
	        {
	            throw new ArgumentNullException("output");
	        }
	        if (!output.CanWrite)
	        {
	            throw new ArgumentException("Stream is not writable.");
	        }
	        stream = output;
	        buffer = new byte[128];
	    }

	    public void Close()
	    {
	        Dispose(true);
	    }

	    public void Dispose()
	    {
	        Dispose(true);
	    }

	    protected virtual void Dispose(bool disposing)
	    {
	        if (disposing)
	        {
	            stream.Close();
	        }
	    }

	    public void Flush()
	    {
	        stream.Flush();
	    }

	    public void Write(byte value)
	    {
	        stream.WriteByte(value);
	    }

	    public void Write(byte[] buf)
	    {
	        if (buf == null)
	        {
	            throw new ArgumentNullException("buf");
	        }
	        stream.Write(buf, 0, buf.Length);
	    }
	        
	    public void Write(double value)
	    {
			ulong num = (ulong) BitConverter.DoubleToInt64Bits(value);
	        buffer[7] = (byte)num;
	        buffer[6] = (byte)(num >> 8);
	        buffer[5] = (byte)(num >> 16);
	        buffer[4] = (byte)(num >> 24);
	        buffer[3] = (byte)(num >> 32);
	        buffer[2] = (byte)(num >> 40);
	        buffer[1] = (byte)(num >> 48);
	        buffer[0] = (byte)(num >> 56);
	        stream.Write(buffer, 0, 8);
	    }

	    public void Write(short value)
	    {
	        buffer[1] = (byte)value;
	        buffer[0] = (byte)(value >> 8);
	        stream.Write(buffer, 0, 2);
	    }

	    public void Write(int value)
	    {
	        buffer[3] = (byte)value;
	        buffer[2] = (byte)(value >> 8);
	        buffer[1] = (byte)(value >> 16);
	        buffer[0] = (byte)(value >> 24);
	        stream.Write(buffer, 0, 4);
	    }

	    public void Write(long value)
	    {
	        buffer[7] = (byte)value;
	        buffer[6] = (byte)(value >> 8);
	        buffer[5] = (byte)(value >> 16);
	        buffer[4] = (byte)(value >> 24);
	        buffer[3] = (byte)(value >> 32);
	        buffer[2] = (byte)(value >> 40);
	        buffer[1] = (byte)(value >> 48);
	        buffer[0] = (byte)(value >> 56);
	        stream.Write(buffer, 0, 8);
	    }

	    public void Write(sbyte value)
	    {
	        stream.WriteByte((byte)value);
	    }

	    public void Write(float value)
	    {
			byte[] bytes = BitConverter.GetBytes(value);
			uint num = BitConverter.ToUInt32(bytes, 0);

	        buffer[3] = (byte)num;
	        buffer[2] = (byte)(num >> 8);
	        buffer[1] = (byte)(num >> 16);
	        buffer[0] = (byte)(num >> 24);
	        stream.Write(buffer, 0, 4);
	    }
	        
	    public void Write(ushort value)
	    {
	        buffer[1] = (byte)value;
	        buffer[0] = (byte)(value >> 8);
	        stream.Write(buffer, 0, 2);
	    }
	        
	    public void Write(uint value)
	    {
	        buffer[3] = (byte)value;
	        buffer[2] = (byte)(value >> 8);
	        buffer[1] = (byte)(value >> 16);
	        buffer[0] = (byte)(value >> 24);
	        stream.Write(buffer, 0, 4);
	    }
	        
	    public void Write(ulong value)
	    {
	        buffer[7] = (byte)value;
	        buffer[6] = (byte)(value >> 8);
	        buffer[5] = (byte)(value >> 16);
	        buffer[4] = (byte)(value >> 24);
	        buffer[3] = (byte)(value >> 32);
	        buffer[2] = (byte)(value >> 40);
	        buffer[1] = (byte)(value >> 48);
	        buffer[0] = (byte)(value >> 56);
	        stream.Write(buffer, 0, 8);
	    }

	    public void Write(byte[] buf, int index, int count)
	    {
	        stream.Write(buf, index, count);
	    }
	}
}