using System;
using System.IO;
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
        public void TestNull()
        {
            TestValue(
                null, 
                (packer, v) => packer.PackNull(),
                unpacker => unpacker.UnpackNull());
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
		    var rand = new Random(0);
		    for (int i = 0; i < 1000; i++) 
            {
			    TestFloat((float)rand.NextDouble());
		    }
	    }

        private static void TestFloat(float val)
        {
            TestValue(
                 val,
                 (packer, v) => packer.PackFloat(v),
                 unpacker => unpacker.UnpackFloat());
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
            var rand = new Random(0);
            for (int i = 0; i < 1000; i++)
            {
                TestDouble(rand.NextDouble());
            }
        }

        private static void TestDouble(double val)
        {
            TestValue(
                 val,
                 (packer, v) => packer.PackDouble(v),
                 unpacker => unpacker.UnpackDouble());
        }

        private static void TestValue<T>(T val, Action<Packer, T> pack, Func<Unpacker, T> unpack)
        {
            var stream = new MemoryStream();
            pack(new Packer(stream), val);

            stream.Seek(0, SeekOrigin.Begin);
            var unpacker = new Unpacker(stream);
            Assert.AreEqual(val, unpack(unpacker));
        }
    }
}
