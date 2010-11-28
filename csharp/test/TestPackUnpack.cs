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
