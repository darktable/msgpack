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
            var stream = new MemoryStream();
            new Packer(stream).Pack(val);

	        stream.Seek(0, SeekOrigin.Begin);
		    var unpacker = new Unpacker(stream);
	        Assert.AreEqual(val, unpacker.UnpackBool());
	    }
    }
}
