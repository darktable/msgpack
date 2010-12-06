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
                        }, unpacker.UnpackListOfObjects());

                Assert.AreEqual(
                    new List<double>
                        {
                            0.0, -0.0, 1.0, -1.0, 3.40282347E+38, -3.40282347E+38, 1.7976931348623157E+308, -1.7976931348623157E+308,
                            double.NaN, double.PositiveInfinity, double.NegativeInfinity
                        }, unpacker.UnpackListOfObjects());

                Assert.AreEqual(0xffffffffffffffff, unpacker.UnpackULong());
                Assert.AreEqual(null, unpacker.UnpackNull());
                Assert.AreEqual(true, unpacker.UnpackBool());
                Assert.AreEqual(false, unpacker.UnpackBool());

                Assert.AreEqual(Enumerable.Range(1, 65535).ToList(), unpacker.UnpackListOfObjects());
                Assert.AreEqual(Enumerable.Range(1, 66000).ToList(), unpacker.UnpackListOfObjects());

                TestPackUnpack.AssertDictionariesEqual(
                    new Dictionary<int, bool> {{1, true}, {10, false}, {-127, true}}, unpacker.UnpackDictionary());

                TestPackUnpack.AssertDictionariesEqual(
                    Enumerable.Range(1, 65536).ToDictionary(v => v, v => v * 2), unpacker.UnpackDictionary());

                TestPackUnpack.AssertDictionariesEqual(
                    new Dictionary<int, string> { { 1, "qwerty" }, { 2, "zxc" }, { 5, "" }, {6, null} }, UnpackDictionary(unpacker));
            }
        }

        private static Dictionary<int, string> UnpackDictionary(Unpacker unpacker)
        {
            int length = unpacker.UnpackMap();
            var dict = new Dictionary<int, string>(length);
            for (int i = 0; i < length; i++)
            {
                dict.Add(unpacker.UnpackInt(), unpacker.UnpackString());
            }
            return dict;
        }
    }
}
