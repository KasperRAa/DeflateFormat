using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat.Codes
{
    internal class LiteralCode : Code
    {
        public byte Value;

        public LiteralCode(int value)
        {
            Value = (byte)value;
        }
    }
}
