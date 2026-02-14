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
        [DataRow(CompressionMethod.Optimal, 259, 100, null)]
        [DataRow(CompressionMethod.Optimal, 2, 100, null)]
        public void TestInvalidArgumentMaxLength(CompressionMethod method, int maxLength, int maxDistance, int? maxBlockLength)
        {
            new Deflate(method, maxLength, maxDistance, maxBlockLength);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [DataRow(CompressionMethod.Optimal, 100, 32769, null)]
        [DataRow(CompressionMethod.Optimal, 100, 0, null)]
        public void TestInvalidArgumentMaxDistance(CompressionMethod method, int maxLength, int maxDistance, int? maxBlockLength)
        {
            new Deflate(method, maxLength, maxDistance, maxBlockLength);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [DataRow(CompressionMethod.Optimal, 100, 100, 0)]
        public void TestInvalidArgumentMaxBlockLength(CompressionMethod method, int maxLength, int maxDistance, int? maxBlockLength)
        {
            new Deflate(method, maxLength, maxDistance, maxBlockLength);
        }
    }
}
