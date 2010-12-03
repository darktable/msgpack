using System;
using System.Collections.Generic;
using System.IO;

public class Unpacker
{
    private const int DefaultBufferSize = 32*1024;
    private readonly BufferedUnpackerImpl impl;
    private readonly Stream stream;

    protected int bufferReserveSize;
    protected int parsed;

    public Unpacker() : this(DefaultBufferSize)
    {
    }

    public Unpacker(int bufferReserveSize) : this(null, bufferReserveSize)
    {
    }

    public Unpacker(Stream stream)
        : this(stream, DefaultBufferSize)
    {
    }

    public Unpacker(Stream stream, int bufferReserveSize)
    {
        parsed = 0;
        this.bufferReserveSize = bufferReserveSize/2;
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

    public string UnpackString()
    {
        return impl.UnpackString();
    }

    public byte[] UnpackBytes()
    {
        return impl.UnpackBytes();
    }

    public ulong UnpackULong()
    {
        return impl.UnpackULong();
    }

    public long UnpackLong()
    {
        return impl.UnpackLong();
    }

    public uint UnpackUInt()
    {
        return impl.UnpackUInt();
    }

    public int UnpackInt()
    {
        return impl.UnpackInt();
    }

    public ushort UnpackUShort()
    {
        return impl.UnpackUShort();
    }

    public short UnpackShort()
    {
        return impl.UnpackShort();
    }

    public byte UnpackByte()
    {
        return impl.UnpackByte();
    }

    public sbyte UnpackSByte()
    {
        return impl.UnpackSByte();
    }

    public object UnpackObject()
    {
        return impl.UnpackObject();
    }

    public bool TryUnpackObject(out object result)
    {
        return impl.TryUnpackObject(out result);
    }

    public int UnpackArray()
    {
        return impl.UnpackArray();
    }

    public TValue Unpack<TValue>() where TValue : class, IMessagePackable, new()
    {
        return impl.Unpack<TValue>(this);
    }

    public char UnpackChar()
    {
        return impl.UnpackChar();
    }

    public T UnpackEnum<T>()
    {
        return impl.UnpackEnum<T>();
    }

    public List<object> UnpackObjectList()
    {
        return impl.UnpackObjectList();
    }

    public List<T> UnpackList<T>() where T : class, IMessagePackable, new()
    {
        return impl.UnpackList<T>(this);
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

        int nextSize = impl.buffer.Length*2;
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