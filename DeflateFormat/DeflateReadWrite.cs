using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat
{
    internal static class DeflateReadWrite
    {
        public static bool ReadBit(IReadOnlyList<byte> bytes, ref int position) => ((bytes[position / 8] >> (position++ % 8)) & 1) == 1;
        public static int ReadInt(IReadOnlyList<byte> bytes, ref int position, int bits)
        {
            int n = 0;
            for (int i = 0; i < bits; i++)
            {
                if (ReadBit(bytes, ref position)) n |= 1 << i;
            }
            return n;
        }
    }
}
