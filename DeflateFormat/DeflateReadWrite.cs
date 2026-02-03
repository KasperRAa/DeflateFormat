using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat
{
    internal static class DeflateReadWrite
    {
        public static bool ReadBit(IReadOnlyList<byte> bytes, ref int position)
        {
            byte @byte = bytes[position / 8];
            int bit = position % 8;
            bool value = ((@byte >> bit) & 1) == 1;
            position++;
            return value;
        }
        public static int ReadInt(IReadOnlyList<byte> bytes, ref int position, int bits)
        {
            int n = 0;
            for (int i = 0; i < bits; i++)
            {
                if (ReadBit(bytes, ref position)) n |= 1 << i;
            }
            return n;
        }

        public static void WriteBit(List<byte> bytes, ref int position, bool value)
        {
            if (position % 8 == 0) bytes.Add(0);
            int n = position / 8;
            bytes[n] |= (byte)((value ? 1 : 0) << position % 8);
            position++;
        }
        public static void WriteInt(List<byte> bytes, ref int position, int value, int bits)
        {
            for (int i = 0; i < bits; i++) WriteBit(bytes, ref position, ((value >>> i) & 1) == 1);
        }
    }
}
