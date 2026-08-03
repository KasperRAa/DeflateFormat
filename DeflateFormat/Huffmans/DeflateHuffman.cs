using DeflateFormat.Codes;
using DeflateFormat.Huffmans.Nodes;
using HuffmanTreeInts;
using HuffmanTreeBytes;
using System;
using System.Collections;
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

        private static string[] GetSequencesFromLengths(IReadOnlyList<byte> lengths)
        {
            if (lengths.Count == 0) return new string[0];

            int lengthCap = lengths.Max() + 1;
            int[] valueByLength = new int[lengthCap];

            int[] lengthCounts = new int[lengthCap];
            foreach (byte l in lengths) lengthCounts[l]++;
            lengthCounts[0] = 0;

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
                if (length == 0) { sequences[l] = ""; continue; }//Unused Length. Mark as such and skip.
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

        #region Write Code Functions
        internal void Write(List<byte> result, ref int position, CodeSequence codeSequence)
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
        #endregion

        internal static DeflateHuffman GetStatic()
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

        internal static DeflateHuffman ReadDynamic(byte[] bytes, ref int position)
        {
            int HLIT = DeflateReadWrite.ReadInt(bytes, ref position, 5) + 257;
            int HDIST = DeflateReadWrite.ReadInt(bytes, ref position, 5) + 1;
            int HCLEN = DeflateReadWrite.ReadInt(bytes, ref position, 4) + 4;

            int[] readOrder = new int[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15};

            byte[] lengths = new byte[19];

            for (int i = 0; i < HCLEN; i++) lengths[readOrder[i]] = (byte)DeflateReadWrite.ReadInt(bytes, ref position, 3);

            string[] decodeStrings = GetSequencesFromLengths(lengths);
            StandardHuffman decoder = new StandardHuffman(decodeStrings);

            List<byte> litLengths = new();
            DecodeDynamicLengths(bytes, ref position, litLengths, HLIT, decoder);

            List<byte> disLengths = new();
            DecodeDynamicLengths(bytes, ref position, disLengths, HDIST, decoder);

            return new DeflateHuffman(litLengths, disLengths);
        }
        private static void DecodeDynamicLengths(byte[] bytes, ref int position, List<byte> lengths, int goal, StandardHuffman decoder)
        {
            while (lengths.Count < goal)
            {
                int code = decoder.Read(bytes, ref position);
                if (code < 16) lengths.Add((byte)code);
                else if (code == 16)
                {
                    byte lastCode = lengths.Last();
                    int repeat = DeflateReadWrite.ReadInt(bytes, ref position, 2) + 3;
                    for (int i = 0; i < repeat; i++) lengths.Add(lastCode);
                }
                else if (code == 17)
                {
                    int zeroes = DeflateReadWrite.ReadInt(bytes, ref position, 3) + 3;
                    for (int i = 0; i < zeroes; i++) lengths.Add(0);
                }
                else if (code == 18)
                {
                    int zeroes = DeflateReadWrite.ReadInt(bytes, ref position, 7) + 11;
                    for (int i = 0; i < zeroes; i++) lengths.Add(0);
                }
                else throw new Exception($"Unknown Code {code}");
            }
            if (lengths.Count != goal) throw new Exception("Decoded Dynamic Length Incorrectly.");
        }

        internal static DeflateHuffman GetDynamic(CodeSequence codeSequence)
        {
            List<int> litLens = new List<int>();
            List<int> dists = new List<int>();
            foreach (Code code in codeSequence.GetCodes())
            {
                switch (code)
                {
                    case LiteralCode:
                        litLens.Add(((LiteralCode)code).Value);
                        break;
                    case EndCode:
                        litLens.Add(256);
                        break;
                    case CompressedCode:
                        var comCode = (CompressedCode)code;
                        litLens.Add(comCode.LengthCode);
                        dists.Add(comCode.DistanceCode);
                        break;
                    default:
                        throw new Exception("Unknown Code");
                }
            }

            Dictionary<int, string> litLenDict;
            var literalLengthTree = HuffmanTreeInts.Tree.GetTreeFromArray(litLens);
            List<byte> litLengths = new();
            litLenDict = literalLengthTree.GetDictionary();
            for (int i = 0; i < 286; i++)
            {
                if (!litLenDict.ContainsKey(i)) litLengths.Add(0);
                else litLengths.Add((byte)litLenDict[i].Length);
            }

            List<byte> disLengths = new();
            if (dists.Count > 0)
            {
                Dictionary<int, string> disDict;
                var distanceTree = HuffmanTreeInts.Tree.GetTreeFromArray(dists);
                disDict = distanceTree.GetDictionary();
                for (int i = 0; i < 30; i++)
                {
                    if (!disDict.ContainsKey(i)) disLengths.Add(0);
                    else disLengths.Add((byte)disDict[i].Length);
                }

                if (disDict.Count == 1)
                {
                    if (disLengths[0] == 0) disLengths[0] = 1;
                    else disLengths[1] = 1;
                }
            }

            return new DeflateHuffman(litLengths, disLengths);
        }

        internal void WriteDynamic(List<byte> bytes, ref int position)
        {
            #region Get Values
            var litLenDict = _litHuffman.GetDictionary();
            var distDict = _disHuffman.GetDictionary();
            int HLIT = litLenDict.Max(x => x.Key) + 1;
            int HDIST = distDict.Max(x => x.Key) + 1;


            var litLenCodeLengths = FillCodeLengths(HLIT, litLenDict);
            var distCodeLengths = FillCodeLengths(HDIST, distDict);
            var codeLengthList = new List<LengthCode>();
            codeLengthList.AddRange(litLenCodeLengths);
            codeLengthList.AddRange(distCodeLengths);

            var codeBytes = codeLengthList.ConvertAll(x => x.Code);

            //Make a huffman tree of the used lengths
            var tree = HuffmanTreeBytes.Tree.GetTreeFromArray(codeBytes);
            var intermediateStep = tree.GetDictionary();
            int iEnd = intermediateStep.Max(x => x.Key);
            var codeLengths = new List<byte>();
            for (int i = 0; i <= iEnd; i++) codeLengths.Add(intermediateStep.TryGetValue((byte)i, out string code) ? (byte)code.Length : (byte)0);
            string[] codeSequences = GetSequencesFromLengths(codeLengths);

            var encoder = new StandardHuffman(codeSequences);

            //Get all of the lengths used
            var encoderDict = encoder.GetDictionary();
            var encoderLengths = encoderDict.ToList().ConvertAll(x => x.Value.Length);

            //Get and write HCLEN
            int[] readOrder = new int[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };
            int HCLEN = 19;
            while (HCLEN > 1)
            {
                if (encoderDict.ContainsKey(readOrder[HCLEN - 1])) break;
                HCLEN--;
            }
            #endregion

            DeflateReadWrite.WriteInt(bytes, ref position, HLIT - 257, 5);
            DeflateReadWrite.WriteInt(bytes, ref position, HDIST - 1, 5);
            DeflateReadWrite.WriteInt(bytes, ref position, HCLEN - 4, 4);

            for (int i = 0; i < HCLEN; i++)
            {
                int value;
                if (encoderDict.TryGetValue(readOrder[i], out string v)) value = v.Length;
                else value = 0;

                DeflateReadWrite.WriteInt(bytes, ref position, value, 3);
            }

            WriteCodes(bytes, ref position, encoder, litLenCodeLengths, HLIT);
            WriteCodes(bytes, ref position, encoder, distCodeLengths, HDIST);
        }
        private List<LengthCode> FillCodeLengths(int goal, Dictionary<int, string> lengths)
        {
            var codeLenList = new List<LengthCode>();

            List<LengthCode> tempList = new();
            for (int i = 0; i < goal; i++)
            {
                byte code = lengths.ContainsKey(i) ? (byte)lengths[i].Length : (byte)0;
                tempList.Add(new LengthCode(code));
            }

            //Compress Codes
            for (int i = 0; i < tempList.Count; i++)
            {
                LengthCode code = tempList[i];

                while (i + 1 < tempList.Count && code.Code == tempList[i + 1].Code)
                {
                    code.Count++;
                    tempList.RemoveAt(i + 1);
                }
            }

            //Modify Codes
            int? lastCode = null;
            foreach (var code in tempList)
            {
                if (code.Count < 3 || code.Count == 3 && (!lastCode.HasValue || lastCode.Value != code.Code))
                {
                    lastCode = code.Code;
                    for (int i = 0; i < code.Count; i++) codeLenList.Add(new LengthCode(code.Code));
                }
                else
                {
                    while (code.Count > 0)
                    {
                        if (code.Code > 0)//Code 16
                        {
                            if (!lastCode.HasValue || lastCode.Value != code.Code)
                            {
                                code.Count--;
                                lastCode = code.Code;
                                codeLenList.Add(code);
                            }
                            byte countDelta = Math.Min(code.Count, (byte)6);
                            for (int i = 0; i < code.Count; i++)
                            {
                                codeLenList.Add(new LengthCode(code.Code, code.Count));
                                code.Count--;
                            }
                        }
                        else if (code.Count < 11)//Code 17
                        {
                            codeLenList.Add(new LengthCode(17, code.Count));
                            code.Count = 0;
                        }
                        else //Code 18
                        {
                            byte countDelta = Math.Min(code.Count, (byte)138);
                            codeLenList.Add(new LengthCode(18, countDelta));
                            code.Count -= countDelta;
                        }
                    }
                }
            }

            return codeLenList;
        }
        private class LengthCode
        {
            public byte Code { get; set; }
            public byte Count { get; set; }

            public LengthCode(byte code) : this(code, 1)
            { }
            public LengthCode(byte code, byte count)
            {
                Code = code;
                Count = count;
            }

            public override string ToString() => $"[{Code}, {Count}]";
        }
        private void WriteCodes(List<byte> bytes, ref int position, StandardHuffman encoder, List<LengthCode> lengthCodes, int goal)
        {
            byte lastLength = 0;
            foreach (var lengthCode in lengthCodes)
            {
                encoder.Write(bytes, ref position, lengthCode.Code);

                if (lengthCode.Code == 16) DeflateReadWrite.WriteInt(bytes, ref position, lengthCode.Count - 3, 2);
                else if (lengthCode.Code == 17) DeflateReadWrite.WriteInt(bytes, ref position, lengthCode.Count - 3, 3);
                else if (lengthCode.Code == 18) DeflateReadWrite.WriteInt(bytes, ref position, lengthCode.Count - 11, 7);
                else lastLength = lengthCode.Code;
            }
        }

        internal void EstimateSize(ref long bitSize, CodeSequence codeSequence)
        {
            Dictionary<int, string> tempDict;

            tempDict = _litHuffman.GetDictionary();
            var litDict = new Dictionary<int, int>();
            foreach (var pair in tempDict) litDict.Add(pair.Key, pair.Value.Length);

            tempDict = _disHuffman.GetDictionary();
            var disDict = new Dictionary<int, int>();
            foreach (var pair in tempDict) disDict.Add(pair.Key, pair.Value.Length);

            foreach (var code in codeSequence.GetCodes())
            {
                switch (code)
                {
                    case LiteralCode:
                        bitSize += litDict[((LiteralCode)code).Value];
                        break;
                    case CompressedCode:
                        var compCode = (CompressedCode)code;
                        bitSize += litDict[compCode.LengthCode];
                        bitSize += compCode.GetExtraBitsForLength();
                        bitSize += disDict[compCode.DistanceCode];
                        bitSize += compCode.GetExtraBitsForDistance();
                        break;
                    case EndCode:
                        bitSize += litDict[EndCode.EndValue];
                        break;
                    default:
                        throw new Exception($"Unknown code: {code.GetType().Name}");
                }
            }
        }
    }
}
