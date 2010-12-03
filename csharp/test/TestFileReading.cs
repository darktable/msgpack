using System.IO;
using NUnit.Framework;

namespace MsgPack.Test
{
    [TestFixture]
    public class TestFileReading
    {
        [Test]
        public void TestReadFromFile()
        {
            var m = new MemoryStream();
            new Packer(m).PackInt(12345678);

            using (var stream = new FileStream(@"TestFiles\test.mpac", FileMode.Open))
            {
                var unpacker = new Unpacker(stream);
                Assert.AreEqual(12345678, unpacker.UnpackInt());
            }
        }
    }
}
