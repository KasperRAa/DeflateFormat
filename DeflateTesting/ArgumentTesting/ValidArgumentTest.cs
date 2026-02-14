using DeflateFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateTesting.ArgumentTesting
{
    [TestClass]
    public class ValidArgumentTest
    {
        [TestMethod]
        [DataRow(CompressionMethod.Optimal, 258, 100, null)]
        [DataRow(CompressionMethod.Optimal, 3, 100, null)]
        public void TestInvalidArgumentMaxLength(CompressionMethod method, int maxLength, int maxDistance, int? maxBlockLength)
        {
            new Deflate(method, maxLength, maxDistance, maxBlockLength);
        }

        [TestMethod]
        [DataRow(CompressionMethod.Optimal, 100, 32768, null)]
        [DataRow(CompressionMethod.Optimal, 100, 1, null)]
        public void TestInvalidArgumentMaxDistance(CompressionMethod method, int maxLength, int maxDistance, int? maxBlockLength)
        {
            new Deflate(method, maxLength, maxDistance, maxBlockLength);
        }

        [TestMethod]
        [DataRow(CompressionMethod.Optimal, 100, 100, 1)]
        [DataRow(CompressionMethod.Optimal, 100, 100, null)]
        public void TestInvalidArgumentMaxBlockLength(CompressionMethod method, int maxLength, int maxDistance, int? maxBlockLength)
        {
            new Deflate(method, maxLength, maxDistance, maxBlockLength);
        }
    }
}
