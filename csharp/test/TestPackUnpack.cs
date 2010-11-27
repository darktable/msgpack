using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using System.Linq;

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
            var outStream = new MemoryStream();
            new Packer(outStream).Pack(val);
            var obj = UnpackOne(outStream);
            Assert.AreEqual(val, obj.AsBool());
	    }

/*        [Test]
        public void TestInt()
        {
            TestInt(0);
            TestInt(-1);
            TestInt(1);
            TestInt(int.MinValue);
            TestInt(int.MaxValue);
            var rand = new Random(0);
            for (int i = 0; i < 1000; i++)
            {
                TestInt(rand.Next(int.MinValue, int.MaxValue));
            }
        }

        private static void TestInt(int val)
        {
            var outStream = new MemoryStream();
            new Packer(outStream).Pack(val);
            var obj = UnpackOne(outStream);
            Assert.AreEqual(val, obj.AsInt());
        }*/

        private static MessagePackObject UnpackOne(MemoryStream outStream)
        {
            var inStream = new MemoryStream(outStream.ToArray());
            var pac = new Unpacker(inStream);
            var enumerable = ((IEnumerable<MessagePackObject>)pac);
            var result = enumerable.ToList();
            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(result[0]);
            return result[0];
        }
    }
}
