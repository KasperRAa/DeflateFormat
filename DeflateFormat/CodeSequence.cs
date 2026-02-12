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
                        int start = bytes.Count;
                        for (int i = 0; i < compressedCode.Length; i++)
                        {
                            int position = start - compressedCode.Distance + i;
                            position %= start;
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
    }
}
