using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateTesting
{
    internal static class InbuiltDeflate
    {
        public static byte[] Compress(byte[] bytes, int bufferSize = 1024)
        {
            using var stream = new MemoryStream();
            var compressor = new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true);
            compressor.Write(bytes);
            compressor.Dispose();
            stream.Position = 0;
            return GetBytesFromStream(stream, bufferSize);
        }

        public static byte[] Decompress(byte[] bytes, int bufferSize = 1024)
        {
            using var stream = new MemoryStream(bytes);
            using var deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
            stream.Position = 0;
            return GetBytesFromStream(deflateStream, bufferSize);
        }

        public static byte[] GetBytesFromStream(Stream stream, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];

            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int bytesRead = stream.Read(buffer.AsSpan(totalRead));
                if (bytesRead == 0) break;
                totalRead += bytesRead;
            }

            return buffer.Take(totalRead).ToArray();
        }
    }
}
