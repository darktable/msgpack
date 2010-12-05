using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace MsgPack.Test
{
    [TestFixture]
    public class TestFileReading
    {
        [Test]
        public void TestReadFromFile()
        {
            using (var stream = new FileStream(@"TestFiles\test.mpac", FileMode.Open))
            {
                var unpacker = new Unpacker(stream);

                Assert.AreEqual(
                    new List<long>
                        {
                            0, 1, -1, -123, 12345678, -31, -128, -65535, -(1L << 16), -(1L << 24), 31, 255, 256, (1L << 16),
                            (1L << 24), (1L << 32)
                        }, unpacker.UnpackObjectList());

                Assert.AreEqual(
                    new List<double>
                        {
                            0.0, -0.0, 1.0, -1.0, 3.40282347E+38, -3.40282347E+38, 1.7976931348623157E+308, -1.7976931348623157E+308,
                            double.NaN, double.PositiveInfinity, double.NegativeInfinity
                        }, unpacker.UnpackObjectList());

                Assert.AreEqual(0xffffffffffffffff, unpacker.UnpackULong());
                Assert.AreEqual(null, unpacker.UnpackNull());
                Assert.AreEqual(true, unpacker.UnpackBool());
                Assert.AreEqual(false, unpacker.UnpackBool());

                Assert.AreEqual(Enumerable.Range(1, 65535).ToList(), unpacker.UnpackObjectList());
                Assert.AreEqual(Enumerable.Range(1, 66000).ToList(), unpacker.UnpackObjectList());
            }
        }
    }
}
