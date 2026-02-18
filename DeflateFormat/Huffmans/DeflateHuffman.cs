using DeflateFormat.Codes;
using DeflateFormat.Huffmans.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat.Huffmans
{
    internal class DeflateHuffman
    {
        private StandardHuffman _litHuffman;
        private StandardHuffman _disHuffman;

        private DeflateHuffman(List<byte> litLengths, List<byte> disLengths)
        {
            string[] litSequences = GetSequencesFromLengths(litLengths);
            string[] disSequences = GetSequencesFromLengths(disLengths);

            _litHuffman = new StandardHuffman(litSequences);
            _disHuffman = new StandardHuffman(disSequences);
        }

        private string[] GetSequencesFromLengths(List<byte> lengths)
        {
            int lengthCap = lengths.Max() + 1;
            int[] valueByLength = new int[lengthCap];

            int[] lengthCounts = new int[lengthCap];
            foreach (byte l in lengths) lengthCounts[l]++;

            for (int length = 1; length < lengthCap; length++)
            {
                int lastValue = valueByLength[length - 1];
                int lastLengthCount = lengthCounts[length - 1];
                valueByLength[length] = (lastValue + lastLengthCount) << 1;
            }

            int lengthCount = lengths.Count;
            string[] sequences = new string[lengthCount];
            for (int l = 0; l < lengthCount; l++)
            {
                int length = lengths[l];
                string sequence = Convert.ToString(valueByLength[length], toBase: 2).PadLeft(length, '0');
                sequences[l] = sequence;
                if (sequences[l].Length != length) throw new Exception($"Huffman Bit Sequence is invalid {{({sequence}.Length != {length}), {l}}}");
                valueByLength[length]++;
            }

            return sequences;
        }

        public CodeSequence Read(IReadOnlyList<byte> bytes, ref int position)
        {
            CodeSequence codeSequence = new CodeSequence();
            while (codeSequence.IsOpen)
            {
                int code = _litHuffman.Read(bytes, ref position);

                if (code < 256) codeSequence.AddCode(new LiteralCode(code));
                if (code == 256) codeSequence.AddCode(new EndCode());
                if (code > 256)
                {
                    int extraLength = DeflateReadWrite.ReadInt(bytes, ref position, CompressedCode.GetExtraBitsForLength(code));
                    int distance = _disHuffman.Read(bytes, ref position);
                    int extraDistance = DeflateReadWrite.ReadInt(bytes, ref position, CompressedCode.GetExtraBitsForDistance(distance));

                    codeSequence.AddCode(new CompressedCode(code, extraLength, distance, extraDistance));
                }
            }
            return codeSequence;
        }

        public void Write(List<byte> result, ref int position, CodeSequence codeSequence)
        {
            foreach (Code code in codeSequence.GetCodes()) WriteCode(result, ref position, code);
        }
        private void WriteCode(List<byte> result, ref int position, Code code)
        {
            switch (code)
            {
                case LiteralCode:
                    WriteLiteralCode(result, ref position, (LiteralCode)code);
                    break;
                case CompressedCode:
                    WriteCompressedCode(result, ref position, (CompressedCode)code);
                    break;
                case EndCode:
                    WriteEndCode(result, ref position);
                    break;
                default:
                    throw new Exception("Unknown Code");
            }
        }
        private void WriteLiteralCode(List<byte> result, ref int position, LiteralCode code)
        {
            _litHuffman.Write(result, ref position, code.Value);
        }
        private void WriteCompressedCode(List<byte> result, ref int position, CompressedCode code)
        {
            _litHuffman.Write(result, ref position, code.LengthCode);
            DeflateReadWrite.WriteInt(result, ref position, code.ExtraLength, code.GetExtraBitsForLength());

            _disHuffman.Write(result, ref position, code.DistanceCode);
            DeflateReadWrite.WriteInt(result, ref position, code.ExtraDistance, code.GetExtraBitsForDistance());
        }
        private void WriteEndCode(List<byte> result, ref int position)
        {
            _litHuffman.Write(result, ref position, EndCode.EndValue);
        }

        public static DeflateHuffman GetStatic()
        {
            List<byte> litLengths = new List<byte>();
            for (int i = 000; i <= 143; i++) litLengths.Add(8);
            for (int i = 144; i <= 255; i++) litLengths.Add(9);
            for (int i = 256; i <= 279; i++) litLengths.Add(7);
            for (int i = 280; i <= 287; i++) litLengths.Add(8);

            List<byte> disLengths = new List<byte>();
            for (int i = 000; i <= 31; i++) disLengths.Add(5);

            return new DeflateHuffman(litLengths, disLengths);
        }
    }
}
