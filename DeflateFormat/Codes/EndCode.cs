using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat.Codes
{
    internal class EndCode : Code
    {
        public static int EndValue
        {
            get
            {
                return 256;
            }
        }
    }
}
