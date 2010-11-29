using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace MsgPack.Test
{
    [TestFixture]
    public class TestPackUnpack
    {
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
            TestValue(
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
            TestLong(byte.MinValue);
            TestLong(sbyte.MaxValue);
            TestLong(sbyte.MinValue);
            TestLong(ushort.MaxValue);
            TestLong(ushort.MinValue);
            TestLong(short.MaxValue);
            TestLong(short.MinValue);
            TestLong(int.MaxValue);
            TestLong(int.MinValue);
            TestLong(uint.MaxValue);
            TestLong(uint.MinValue);
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

        private static void TestValue<T>(T val, Action<Packer, T> pack, Func<Unpacker, T> unpack)
        {
            var stream = new MemoryStream();
            pack(new Packer(stream), val);

            stream.Seek(0, SeekOrigin.Begin);
            var unpacker = new Unpacker(stream);
            Assert.AreEqual(val, unpack(unpacker));
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
}