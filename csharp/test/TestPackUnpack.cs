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
            Repeat(5, rand => TestString(GetRandomString(rand, (1 << 25), (1 << 25) + 100)));
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