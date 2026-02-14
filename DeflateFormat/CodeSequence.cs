using DeflateFormat.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat
{
    internal class CodeSequence
    {
        private List<Code> _codes;

        public bool IsOpen
        {
            get
            {
                return _codes.Count == 0 || _codes.Last().GetType() != typeof(EndCode);
            }
        }

        public CodeSequence()
        {
            _codes = new List<Code>();
        }

        public void AddCode(Code code)
        {
            if (!IsOpen) throw new Exception("Sequence cannot be expanded, as it is closed");
            _codes.Add(code);
        }

        public IReadOnlyList<Code> GetCodes() => _codes;

        public byte[] Decode()
        {
            List<byte> bytes = new List<byte>();

            foreach (Code code in _codes)
            {
                switch (code)
                {
                    case LiteralCode:
                        bytes.Add(((LiteralCode)code).Value);
                        break;
                    case CompressedCode:
                        CompressedCode compressedCode = (CompressedCode)code;
                        int start = bytes.Count - compressedCode.TotalDistance;
                        int repeatLength = compressedCode.TotalDistance;
                        for (int i = 0; i < compressedCode.TotalLength; i++)
                        {
                            int position = start + i % repeatLength;
                            bytes.Add(bytes[position]);
                        }
                        break;
                    case EndCode:
                        break;
                    default:
                        throw new Exception("Unknown Code");
                }
            }

            return bytes.ToArray();
        }

        public static CodeSequence Encode(IReadOnlyList<byte> bytes, int maxLength, int maxDistance)
        {
            CodeSequence codeSequence = new CodeSequence();

            int count = bytes.Count;
            for (int i = 0; i < count; i++)
            {
                CompressedCode? compressedCode = GetCompressedCode(bytes, i);

                if (compressedCode != null)
                {
                    codeSequence.AddCode(compressedCode);
                    i += compressedCode.TotalLength - 1;
                }
                else codeSequence.AddCode(new LiteralCode(bytes[i]));
            }

            codeSequence.AddCode(new EndCode());

            return codeSequence;

            CompressedCode? GetCompressedCode(IReadOnlyList<byte> bytes, int position)
            {
                int bestLength = 0, bestDistance = 0;

                int lengthLimit = Math.Min(maxLength, count - position - 1);
                int distanceLimit = Math.Min(maxDistance, position);

                for (int distance = 1; distance <= distanceLimit; distance++)
                {
                    for (int length = 0; length <= lengthLimit; length++)
                    {
                        int startPosition = position - distance;
                        byte previousByte = bytes[startPosition + length % distance];

                        byte nextByte = bytes[position + length];

                        if (nextByte == previousByte) { bestLength = length; bestDistance = distance; }
                        else break;
                    }
                }

                CompressedCode? result = null;
                if (bestLength >= 3) result = new CompressedCode(bestLength, bestDistance);

                return result;
            }
    }
    }
}
