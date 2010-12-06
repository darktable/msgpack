using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MsgPack.Test
{
    [TestFixture]
    public class TestPackUnpack
    {
        [Test, Ignore]
        public void StressTest()
        {
            var stream = new MemoryStream();
            var packer = new Packer(stream);
            var unpacker = new Unpacker(stream);

            packer.PackULong(ulong.MaxValue);

            for (int i = 0; i < 10000000; i++)
            {
                stream.Seek(0, SeekOrigin.Begin);
                unpacker.UnpackULong();
            }
        }

        [Test]
        public void TestBool()
        {
            TestBool(false);
            TestBool(true);
        }

        private static void TestBool(bool val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackBool(v),
                unpacker => unpacker.UnpackBool());
        }

        [Test]
        public void TestDouble()
        {
            TestDouble(0.0);
            TestDouble(-0.0);
            TestDouble(1.0);
            TestDouble(-1.0);
            TestDouble(double.MaxValue);
            TestDouble(double.MinValue);
            TestDouble(double.NaN);
            TestDouble(double.NegativeInfinity);
            TestDouble(double.PositiveInfinity);
            Repeat(1000, rand => TestDouble(rand.NextDouble()));
        }

        private static void TestDouble(double val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackDouble(v),
                unpacker => unpacker.UnpackDouble());
        }

        [Test]
        public void TestFloat()
        {
            TestFloat(0.0f);
            TestFloat(-0.0f);
            TestFloat(1.0f);
            TestFloat(-1.0f);
            TestFloat(float.MaxValue);
            TestFloat(float.MinValue);
            TestFloat(float.NaN);
            TestFloat(float.NegativeInfinity);
            TestFloat(float.PositiveInfinity);
            Repeat(1000, rand => TestFloat((float) rand.NextDouble()));
        }

        private static void TestFloat(float val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackFloat(v),
                unpacker => unpacker.UnpackFloat());
        }

        [Test]
        public void TestNull()
        {
            TestValue<object, object>(
                null,
                (packer, v) => packer.PackNull(),
                unpacker => unpacker.UnpackNull());
        }

        [Test]
        public void TestString()
        {
            TestString("");
            TestString("a");
            TestString("ab");
            TestString("abc");

            // small size string
            Repeat(100, rand => TestString(GetRandomString(rand, 1, 31)));

            // medium size string
            Repeat(100, rand => TestString(GetRandomString(rand, (1 << 15), (1 << 15) + 100)));

            // large size string
            Repeat(2, rand => TestString(GetRandomString(rand, (1 << 25), (1 << 25) + 100)));
        }

        [Test]
        public void TestNullString()
        {
            TestString(null);
        }

        private static void TestString(string val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackString(v),
                unpacker => unpacker.UnpackString());
        }

        private static string GetRandomString(Random rand, int minLength, int maxLength)
        {
            var sb = new StringBuilder();
            int length = minLength + rand.Next(maxLength - minLength + 1);
            Repeat(length, r => sb.Append((char)('a' + r.Next(26))));
            return sb.ToString();
        }

        [Test]
        public void TestULong()
        {
            TestULong(0);
            TestULong(1);
            TestULong(byte.MaxValue);
            TestULong((ulong)sbyte.MaxValue);
            TestULong(ushort.MaxValue);
            TestULong((ulong)short.MaxValue);
            TestULong(int.MaxValue);
            TestULong(uint.MaxValue);
            TestULong(long.MaxValue);
            TestULong(ulong.MaxValue);
            Repeat(1000, rand => TestULong(NextULong(rand)));
        }

        private static void TestULong(ulong val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackULong(v),
                unpacker => unpacker.UnpackULong());
        }

        public static ulong NextULong(Random rand)
        {
            var buffer = new byte[sizeof(ulong)];
            rand.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        [Test]
        public void TestLong()
        {
            TestLong(0);
            TestLong(1);
            TestLong(-1);
            TestLong(byte.MaxValue);
            TestLong(sbyte.MaxValue);
            TestLong(sbyte.MinValue);
            TestLong(ushort.MaxValue);
            TestLong(short.MaxValue);
            TestLong(short.MinValue);
            TestLong(uint.MaxValue);
            TestLong(int.MaxValue);
            TestLong(int.MinValue);
            TestLong(long.MaxValue);
            TestLong(long.MinValue);
            Repeat(1000, rand => TestLong(NextLong(rand)));
        }

        private static void TestLong(long val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackLong(v),
                unpacker => unpacker.UnpackLong());
        }

        public static long NextLong(Random rand)
        {
            var buffer = new byte[sizeof(long)];
            rand.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        [Test]
        public void TestUInt()
        {
            TestUInt(0);
            TestUInt(1);
            TestUInt(byte.MaxValue);
            TestUInt((uint)sbyte.MaxValue);
            TestUInt(ushort.MaxValue);
            TestUInt((uint)short.MaxValue);
            TestUInt(int.MaxValue);
            TestUInt(uint.MaxValue);
            Repeat(1000, rand => TestUInt(NextUInt(rand)));
        }

        private static void TestUInt(uint val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackUInt(v),
                unpacker => unpacker.UnpackUInt());
        }

        public static uint NextUInt(Random rand)
        {
            var buffer = new byte[sizeof(uint)];
            rand.NextBytes(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        [Test]
        public void TestInt()
        {
            TestInt(0);
            TestInt(1);
            TestInt(-1);
            TestInt(byte.MaxValue);
            TestInt(sbyte.MaxValue);
            TestInt(sbyte.MinValue);
            TestInt(ushort.MaxValue);
            TestInt(short.MaxValue);
            TestInt(short.MinValue);
            TestInt(int.MaxValue);
            TestInt(int.MinValue);
            Repeat(1000, rand => TestInt(rand.Next()));
        }

        private static void TestInt(int val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackInt(v),
                unpacker => unpacker.UnpackInt());
        }

        [Test]
        public void TestUShort()
        {
            for (ushort i = ushort.MinValue; i < ushort.MaxValue; i++)
            {
                TestUShort(i);
            }
        }

        private static void TestUShort(ushort val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackUShort(v),
                unpacker => unpacker.UnpackUShort());
        }

        [Test]
        public void TestShort()
        {
            for (short i = short.MinValue; i < short.MaxValue; i++)
            {
                TestShort(i);
            }
        }

        private static void TestShort(short val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackShort(v),
                unpacker => unpacker.UnpackShort());
        }

        [Test]
        public void TestSByte()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                TestSByte(i);
            }
        }

        private static void TestSByte(sbyte val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackSByte(v),
                unpacker => unpacker.UnpackSByte());
        }

        [Test]
        public void TestByte()
        {
            for (byte i = byte.MinValue; i < byte.MaxValue; i++)
            {
                TestByte(i);
            }
        }

        private static void TestByte(byte val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackByte(v),
                unpacker => unpacker.UnpackByte());
        }

        [Test]
        public void TestBytes()
        {
            TestBytes(null);
            TestBytes(new byte[0]);

            var rand = new Random(0);
            TestBytes(GetBytes(rand, 10));
            TestBytes(GetBytes(rand, 31));
            TestBytes(GetBytes(rand, ushort.MaxValue));
            TestBytes(GetBytes(rand, ushort.MaxValue + 1000));
        }

        private static byte[] GetBytes(Random rand, int length)
        {
            var bytes = new byte[length];
            rand.NextBytes(bytes);
            return bytes;
        }

        private static void TestBytes(byte[] val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackBytes(v),
                unpacker => unpacker.UnpackBytes());
        }

        [Test]
        public void TestChar()
        {
            TestChar(char.MaxValue);
            TestChar(char.MinValue);
            Repeat(1000, rand => TestChar((char)rand.Next()));
        }

        private static void TestChar(char val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackChar(v),
                unpacker => unpacker.UnpackChar());
        }

        [Test]
        public void TestEnum()
        {
            TestEnum(SomeEnum.A);
            TestEnum(SomeEnum.B);
            TestEnum(SomeEnum.C);
            TestEnum(OtherEnum.A);
            TestEnum(OtherEnum.B);
            TestEnum(OtherEnum.C);
        }

        private static void TestEnum<T>(T val)
        {
            TestValue(
                val,
                (packer, v) => packer.PackEnum(v),
                unpacker => unpacker.UnpackEnum<T>());
        }

        [Test]
        public void TestOverflows()
        {
            Assert.Throws(
                typeof (MessagePackOverflowException),
                () => TestValue(
                    double.MaxValue,
                    (packer, v) => packer.PackDouble(v),
                    unpacker => unpacker.UnpackFloat()));
            Assert.Throws(
                typeof (MessagePackOverflowException),
                () => TestValue(
                    ulong.MaxValue,
                    (packer, v) => packer.PackULong(v),
                    unpacker => unpacker.UnpackLong()));
            Assert.Throws(
                typeof (MessagePackOverflowException),
                () => TestValue(
                    uint.MaxValue,
                    (packer, v) => packer.PackUInt(v),
                    unpacker => unpacker.UnpackInt()));
            Assert.Throws(
                typeof (MessagePackOverflowException),
                () => TestValue(
                    ushort.MaxValue,
                    (packer, v) => packer.PackUShort(v),
                    unpacker => unpacker.UnpackShort()));
            Assert.Throws(
                typeof (MessagePackOverflowException),
                () => TestValue(
                    byte.MaxValue,
                    (packer, v) => packer.PackByte(v),
                    unpacker => unpacker.UnpackSByte()));
        }


        [Test]
        public void TestMessagePackable()
        {
            TestValue<DataClass, DataClass>(
                null,
                (packer, val) => packer.Pack(val),
                unpacker => unpacker.Unpack<DataClass>());

            Repeat(
                1000,
                rand =>
                    TestValue(
                        new DataClass
                            {
                                Bool = rand.Next(2) == 1,
                                Double = rand.NextDouble(),
                                Int = rand.Next(int.MinValue, int.MaxValue),
                                Long = NextLong(rand),
                                String = GetRandomString(rand, 0, 10000)
                            },
                        (packer, val) => packer.Pack(val),
                        unpacker => unpacker.Unpack<DataClass>()));
        }

        [Test]
        public void TestObject()
        {
            TestObject(null);
            TestObject(true);
            TestObject(12334545.2342345345);
            TestObject(0.0000456f);

            TestObject(ulong.MaxValue);
            TestObject(long.MaxValue);
            TestObject(uint.MaxValue);
            TestObject(int.MaxValue);
            TestObject(ushort.MaxValue);
            TestObject(short.MaxValue);
            TestObject(byte.MaxValue);
            TestObject(sbyte.MaxValue);
            TestObject(1);
            TestObject(-1);

            TestObject(GetBytes(new Random(0), 0));
            TestObject(GetBytes(new Random(0), 1));
            TestObject(GetBytes(new Random(0), 31));
            TestObject(GetBytes(new Random(0), 12345));

            TestObject(SomeEnum.C, (expected, actual) => Assert.AreEqual((int) expected, actual));
            TestObject('z', (expected, actual) => Assert.AreEqual((int) (char) expected, actual));
            
            TestObject(new DataClass {Bool = true},
                       (expected, actual) => Assert.AreEqual(((DataClass) expected).Bool, actual));

            TestObject("str", (expected, actual) => Assert.AreEqual(Encoding.UTF8.GetBytes((string)expected), actual));

            TestObject(new [] {1, 2, 3});
            TestObject(new List<double> { 123.45, double.MaxValue, double.NegativeInfinity, 0.0, -0.0 });
            TestObject(new Dictionary<int, bool> {{1, false}, {int.MaxValue, true}, {4, true}},
                       (expected, actual) => AssertDictionariesEqual((IDictionary) expected, (IDictionary) actual));
        }

        private static void TestObject(object val, Action<object, object> equalityAssert = null)
        {
            TestValue(
                val,
                (packer, v) => packer.PackObject(v),
                unpacker => unpacker.UnpackObject(),
                equalityAssert);
        }

        [Test]
        public void TestCollection()
        {
            TestValue<ICollection<int>, List<object>>(
                null,
                (packer, list) => packer.PackCollection(list),
                unpacker => unpacker.UnpackObjectList());
            TestValue(
                new List<int> {1, 2, 3}, 
                (packer, list) => packer.PackCollection(list),
                unpacker => unpacker.UnpackObjectList());
            TestValue(
                Enumerable.Range(1, 65535).ToList(),
                (packer, list) => packer.PackCollection(list),
                unpacker => unpacker.UnpackObjectList());
            TestValue(
                Enumerable.Range(1, 65536).ToList(),
                    (packer, list) => packer.PackCollection(list),
                    unpacker => unpacker.UnpackObjectList());
            TestValue(
                new List<float> {123.45f, float.MaxValue, float.NegativeInfinity, 0.0f, -0.0f},
                (packer, list) => packer.PackCollection(list),
                unpacker => unpacker.UnpackObjectList());
            TestValue(
                new List<DataClass>
                    {new DataClass {Bool = true}, new DataClass {Long = 123}, new DataClass {String = "qwerty"}},
                (packer, list) => packer.PackCollection(list),
                unpacker => unpacker.UnpackList<DataClass>());
            TestValue(
                new List<string> { "qwerty", "a", null, "" },
                (packer, list) => packer.PackCollection(list),
                unpacker =>
                    {
                        var length = unpacker.UnpackArray();
                        var list = new List<string>(length);
                        for (int i = 0; i < length; i++)
                        {
                            list.Add(unpacker.UnpackString());
                        }
                        return list;
                    });
        }

        [Test]
        public void TestDictionary()
        {
            TestValue<IDictionary, IDictionary<object, object>>(
                null,
                (packer, dic) => packer.PackDictionary(dic),
                unpacker => unpacker.UnpackDictionary());

            TestValue(
                new Dictionary<int, bool> {{1, false}, {int.MaxValue, true}, {4, true}},
                (packer, dic) => packer.PackDictionary(dic),
                unpacker => unpacker.UnpackDictionary(), 
                AssertDictionariesEqual);

            TestValue(
                new Dictionary<string, DataClass>
                    {
                        {"str1", new DataClass {Bool = true, String = "qq"}},
                        {"qwerty", new DataClass {String = "ppp", Long = long.MaxValue}}
                    },
                (packer, dict) => packer.PackDictionary(dict),
                unpacker =>
                    {
                        int length = unpacker.UnpackMap();
                        var dict = new Dictionary<string, DataClass>(length);
                        for (int i = 0; i < length; i++)
                        {
                            dict.Add(unpacker.UnpackString(), unpacker.Unpack<DataClass>());
                        }
                        return dict;
                    });
        }

        public static void AssertDictionariesEqual(IDictionary expected, IDictionary actual)
        {
            Assert.AreEqual(expected.Keys, actual.Keys);
            Assert.AreEqual(expected.Values, actual.Values);
        }

        private static void TestValue<T1, T2>(T1 val, Action<Packer, T1> pack, Func<Unpacker, T2> unpack,
                                         Action<T1, T2> equalityAssert = null)
        {
            var stream = new MemoryStream();
            pack(new Packer(stream), val);

            stream.Seek(0, SeekOrigin.Begin);
            var unpacker = new Unpacker(stream);
            var unpacked = unpack(unpacker);
            if (equalityAssert != null)
            {
                equalityAssert(val, unpacked);
            }
            else
            {
                Assert.AreEqual(val, unpacked);
            }
        }

        private static void Repeat(int times, Action<Random> action)
        {
            var rand = new Random(0);
            for (int i = 0; i < times; i++)
            {
                action(rand);
            }
        }
    }

    internal enum SomeEnum
    {
        A, B, C
    }

    internal enum OtherEnum
    {
        A = 34,
        B = 0,
        C = int.MaxValue
    }

    internal class DataClass: IMessagePackable
    {
        public bool Bool { get; set; }
        public string String { get; set; }
        public int Int { get; set; }
        public long Long { get; set; }
        public double Double { get; set; }
        
        public void ToMsgPack(Packer p)
        {
            p.PackBool(Bool);
            p.PackString(String);
            p.PackInt(Int);
            p.PackLong(Long);
            p.PackDouble(Double);
        }

        public void FromMsgPack(Unpacker u)
        {
            Bool = u.UnpackBool();
            String = u.UnpackString();
            Int = u.UnpackInt();
            Long = u.UnpackLong();
            Double = u.UnpackDouble();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof (DataClass))
            {
                return false;
            }
            var other = (DataClass) obj;
            return other.Bool.Equals(Bool) && Equals(other.String, String) && other.Int == Int && other.Long == Long && other.Double.Equals(Double);
        }
    }
}