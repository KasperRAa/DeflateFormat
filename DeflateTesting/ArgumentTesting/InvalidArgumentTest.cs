using DeflateFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateTesting.ArgumentTesting
{
    [TestClass]
    public class InvalidArgumentTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [DataRow(CompressionMethod.Optimal, 259, 100, 100)]
        [DataRow(CompressionMethod.Optimal, 2, 100, 100)]
        public void TestInvalidArgumentMaxLength(CompressionMethod method, int maxLength, int maxDistance, int? maxBlockLength)
        {
            new Deflate(method, maxLength, maxDistance, maxBlockLength);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [DataRow(CompressionMethod.Optimal, 100, 32769, 100)]
        [DataRow(CompressionMethod.Optimal, 100, 0, 100)]
        public void TestInvalidArgumentMaxDistance(CompressionMethod method, int maxLength, int maxDistance, int? maxBlockLength)
        {
            new Deflate(method, maxLength, maxDistance, maxBlockLength);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [DataRow(CompressionMethod.Optimal, 100, 100, 32767)]
        public void TestInvalidArgumentMaxBlockLength(CompressionMethod method, int maxLength, int maxDistance, int? maxBlockLength)
        {
            new Deflate(method, maxLength, maxDistance, maxBlockLength);
        }
    }
}
