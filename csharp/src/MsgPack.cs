internal static class MsgPack
{
    public const byte BoolFalseType = 0xc2;
    public const byte BoolTrueType = 0xc3;
    public const byte NilType = 0xc0;
    public const byte FloatType = 0xca;
    public const byte DoubleType = 0xcb;
    public const byte Raw16Type = 0xda;
    public const byte Raw32Type = 0xdb;
    public const byte Array16Type = 0xdc;
    public const byte Array32Type = 0xdd;
    public const byte Map16Type = 0xde;
    public const byte Map32Type = 0xdf;
    public const byte UInt8Type = 0xcc;
    public const byte UInt16Type = 0xcd;
    public const byte UInt32Type = 0xce;
    public const byte UInt64Type = 0xcf;
    public const byte Int8Type = 0xd0;
    public const byte Int16Type = 0xd1;
    public const byte Int32Type = 0xd2;
    public const byte Int64Type = 0xd3;

    public const byte MaxFixnum = (1 << 7) - 1;
    public const sbyte MinFixnum = -(1 << 5);
    public const uint MaxUInt8 = (1 << 8) - 1;
    public const ulong MaxUInt16 = (1L << 16) - 1;
    public const ulong MaxUInt32 = (1L << 32) - 1;
    public const int MaxFixRawLength = 31;
    public const int MaxRaw16Length = 65535;
    public const int MaxFixArrayLength = 15;
    public const int MaxArray16Length = 65535;
    public const int MaxFixMapLength = 15;
    public const int MaxMap16Length = 65535;
    public const long MinInt32 = -(1L << 31);
    public const int MinInt8 = -(1 << 7);

    public static bool IsPositiveFixnum(byte b)
    {
        return (b & 0x80) == 0;
    }

    public static bool IsNegativeFixnum(byte b)
    {
        return (b & 0xe0) == 0xe0;
    }

    public static bool IsFixRawType(byte b)
    {
        return (b & 0xe0) == 0xa0;
    }

    public static byte PackFixRawLength(int n)
    {
        return (byte) (0xa0 | n);
    }

    public static int UnpackFixRawLength(byte b)
    {
        return b & 0x1f;
    }

    public static bool IsFixArrayType(byte b)
    {
        return (b & 0xf0) == 0x90;
    }

    public static byte PackFixArayLength(int n)
    {
        return (byte) (0x90 | n);
    }

    public static int UnpackFixArrayLength(byte b)
    {
        return b & 0x0f;
    }

    public static bool IsFixMapType(byte b)
    {
        return (b & 0xf0) == 0x80;
    }

    public static byte PackFixMapLength(int n)
    {
        return (byte) (0x80 | n);
    }

    public static int UnpackFixMapLength(byte b)
    {
        return b & 0x0f;
    }
}
