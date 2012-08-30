using System;
using System.IO;

namespace MsgPack {
	internal class BinaryReaderBigEndian: IDisposable
	{
	    private byte[] buffer;
	    private Stream stream;

	    public BinaryReaderBigEndian(Stream input)
	    {
	        if (input == null)
	        {
	            throw new ArgumentNullException("input");
	        }
	        if (!input.CanRead)
	        {
	            throw new ArgumentException("Stream not readable.");
	        }
	        stream = input;
	        buffer = new byte[128];
	    }

	    public Stream BaseStream
	    {
	        get
	        {
	            return stream;
	        }
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
	            Stream saveStream = stream;
	            stream = null;
	            if (saveStream != null)
	            {
	                saveStream.Close();
	            }
	        }
	        stream = null;
	        buffer = null;
	    }

	    protected void FillBuffer(int numBytes)
	    {
	        if ((buffer != null) && ((numBytes < 0) || (numBytes > buffer.Length)))
	        {
	            throw new ArgumentOutOfRangeException("numBytes");
	        }
	        int offset = 0;
	        int b;
	        if (stream == null)
	        {
	            ThrowFileNotOpen();
	        }
	        if (numBytes == 1)
	        {
	            b = stream.ReadByte();
	            if (b == -1)
	            {
	                ThrowEndOfStream();
	            }
	            buffer[0] = (byte) b;
	        }
	        else
	        {
	            do
	            {
	                b = stream.Read(buffer, offset, numBytes - offset);
	                if (b == 0)
	                {
	                    ThrowEndOfStream();
	                }
	                offset += b;
	            }
	            while (offset < numBytes);
	        }
	    }

	    public int Read(byte[] buf, int index, int count)
	    {
	        if (stream == null)
	        {
	            ThrowFileNotOpen();
	        }
	        return stream.Read(buf, index, count);
	    }

	    public byte ReadByte()
	    {
	        if (stream == null)
	        {
	            ThrowFileNotOpen();
	        }
	        int num = stream.ReadByte();
	        if (num == -1)
	        {
	            ThrowEndOfStream();
	        }
	        return (byte) num;
	    }
	    
	    public byte[] ReadBytes(int count)
	    {
	        if (stream == null)
	        {
	            ThrowFileNotOpen();
	        }
	        byte[] buf = new byte[count];
	        int offset = 0;
	        do
	        {
	            int num2 = stream.Read(buf, offset, count);
	            if (num2 == 0)
	            {
	                break;
	            }
	            offset += num2;
	            count -= num2;
	        }
	        while (count > 0);
	        if (offset != buf.Length)
	        {
	            byte[] dst = new byte[offset];
	            Buffer.BlockCopy(buf, 0, dst, 0, offset);
	            buf = dst;
	        }
	        return buf;
	    }

	    public float ReadSingle()
	    {
			uint num = ReadUInt32();        
			var bytes = BitConverter.GetBytes(num);

			return BitConverter.ToSingle(bytes, 0);
	    }

	    public double ReadDouble()
	    {
	        ulong num = ReadUInt64();

			return BitConverter.Int64BitsToDouble((long) num);
	    }	

	    public short ReadInt16()
	    {
	        FillBuffer(2);
	        return (short) (buffer[1] | (buffer[0] << 8));
	    }

	    public int ReadInt32()
	    {
	        FillBuffer(4);
	        int res = buffer[0];
	        res = unchecked((res << 8) | buffer[1]);
	        res = unchecked((res << 8) | buffer[2]);
	        res = unchecked((res << 8) | buffer[3]);
	        return res;
	    }

	    public long ReadInt64()
	    {
	        FillBuffer(8);
	        long res = buffer[0];
	        res = unchecked((res << 8) | buffer[1]);
	        res = unchecked((res << 8) | buffer[2]);
	        res = unchecked((res << 8) | buffer[3]);
	        res = unchecked((res << 8) | buffer[4]);
	        res = unchecked((res << 8) | buffer[5]);
	        res = unchecked((res << 8) | buffer[6]);
	        res = unchecked((res << 8) | buffer[7]);
	        return res;
	    }

	    public sbyte ReadSByte()
	    {
	        FillBuffer(1);
	        return (sbyte) buffer[0];
	    }

	    public ushort ReadUInt16()
	    {
	        return (ushort) ReadInt16();
	    }
	   
	    public uint ReadUInt32()
	    {
	        return (uint) ReadInt32();
	    }
	   
	    public ulong ReadUInt64()
	    {
	        return (ulong)ReadInt64();
	    }

	    private static void ThrowFileNotOpen()
	    {
	        throw new ObjectDisposedException(null, "File not open");
	    }

	    private static void ThrowEndOfStream()
	    {
	        throw new EndOfStreamException();
	    }
	}
}