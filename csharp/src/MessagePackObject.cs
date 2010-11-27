using System;
using System.Collections.Generic;

public abstract class MessagePackObject: IMessagePackable, ICloneable
{
    public abstract void MessagePack(Packer packer);

    public bool AsBool()
    {
        throw new MessageTypeException("type error");
    }

    
	public bool IsNil() {
		return false;
	}

	public bool IsboolType() {
		return false;
	}

	public bool IsIntegerType() {
		return false;
	}

	public bool IsFloatType() {
		return false;
	}

	public bool IsArrayType() {
		return false;
	}

	public bool IsMapType() {
		return false;
	}

	public bool IsRawType() {
		return false;
	}

	public bool Asbool() {
		throw new MessageTypeException("type error");
	}

	public byte AsByte() {
		throw new MessageTypeException("type error");
	}

	public short AsShort() {
		throw new MessageTypeException("type error");
	}

	public int AsInt() {
		throw new MessageTypeException("type error");
	}

	public long AsLong() {
		throw new MessageTypeException("type error");
	}

//	public BigInteger AsBigInteger() {
//		throw new MessageTypeException("type error");
//	}

	public float AsFloat() {
		throw new MessageTypeException("type error");
	}

	public double AsDouble() {
		throw new MessageTypeException("type error");
	}

	public byte[] AsByteArray() {
		throw new MessageTypeException("type error");
	}

	public string AsString() {
		throw new MessageTypeException("type error");
	}

	public MessagePackObject[] AsArray() {
		throw new MessageTypeException("type error");
	}

	public List<MessagePackObject> AsList() {
		throw new MessageTypeException("type error");
	}

	public Dictionary<MessagePackObject, MessagePackObject> AsMap() {
		throw new MessageTypeException("type error");
	}

	public byte ByteValue() {
		throw new MessageTypeException("type error");
	}

	public short ShortValue() {
		throw new MessageTypeException("type error");
	}

	public int IntValue() {
		throw new MessageTypeException("type error");
	}

	public long LongValue() {
		throw new MessageTypeException("type error");
	}

//	public BigInteger BigIntegerValue() {
//		throw new MessageTypeException("type error");
//	}

	public float FloatValue() {
		throw new MessageTypeException("type error");
	}

	public double DoubleValue() {
		throw new MessageTypeException("type error");
	}

    public abstract object Clone();

//    public object Convert(Template tmpl)  {
//		return convert(tmpl, null);
//	}
//
//	public T Convert<T>(Template tmpl, T to)  {
//		return (T)tmpl.convert(this, to);
//	}
//
//	public T Convert<T>(Class<T> klass)  {
//		return convert(klass, null);
//	}
//
//	public T Convert<T>(T to)  {
//		return convert((Class<T>)to.getClass(), to);
//	}
//
//	private T Convert<T>(Class<T> klass, T to)  {
		// FIXME nullable?
//		return (T)convert(new NullableTemplate(new ClassTemplate(klass)), to);
//	}
}
