using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat.Huffmans.Nodes
{
    internal class LeafNode : Node
    {
        public int Value;

        public LeafNode(int value)
        {
            Value = value;
        }
    }
}
