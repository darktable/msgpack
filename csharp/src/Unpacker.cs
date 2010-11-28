using System;
using System.IO;

public class Unpacker
{
    private readonly Stream stream;
    private readonly BufferedUnpackerImpl impl;

    private const int DefaultBufferSize = 32*1024;

	protected int parsed;
	protected int bufferReserveSize;

    public Unpacker() :this(DefaultBufferSize)
    {
    }

    public Unpacker(int bufferReserveSize): this(null, bufferReserveSize)
    {
    }

    public Unpacker(Stream stream)
        : this(stream, DefaultBufferSize)
    {
    }

    public Unpacker(Stream stream, int bufferReserveSize)
    {
        parsed = 0;
        this.bufferReserveSize = bufferReserveSize / 2;
        this.stream = stream;
        impl = new BufferedUnpackerImpl(
            () =>
                {
                    if (stream == null)
                    {
                        return false;
                    }
                    ReserveBuffer(bufferReserveSize);
                    int rl = this.stream.Read(impl.buffer, impl.filled, impl.buffer.Length - impl.filled);
                    if (rl <= 0)
                    {
                        return false;
                    }
                    BufferConsumed(rl);
                    return true;
                });
    }

    public bool UnpackBool()
    {
        return impl.UnpackBool();
    }

    public object UnpackNull() 
    {
		return impl.UnpackNull();
	}

    public float UnpackFloat()
    {
		return impl.UnpackFloat();
	}

	public double UnpackDouble()
    {
		return impl.UnpackDouble();
	}

    public void BufferConsumed(int size)
    {
        impl.filled += size;
    }

    public void ReserveBuffer(int require)
    {
        if (impl.buffer == null)
        {
            int nextSize1 = (bufferReserveSize < require) ? require : bufferReserveSize;
            impl.buffer = new byte[nextSize1];
            return;
        }

        if (impl.filled <= impl.offset)
        {
            // rewind the buffer
            impl.filled = 0;
            impl.offset = 0;
        }

        if (impl.buffer.Length - impl.filled >= require)
        {
            return;
        }

        int nextSize = impl.buffer.Length * 2;
        int notParsed = impl.filled - impl.offset;
        while (nextSize < require + notParsed)
        {
            nextSize *= 2;
        }

        var tmp = new byte[nextSize];
        Array.Copy(impl.buffer, impl.offset, tmp, 0, impl.filled - impl.offset);

        impl.buffer = tmp;
        impl.filled = notParsed;
        impl.offset = 0;
    }
}
