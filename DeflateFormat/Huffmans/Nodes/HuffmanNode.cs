using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat.Huffmans.Nodes
{
    internal class HuffmanNode
    {
        public Node Node;

        public HuffmanNode()
        {
            Node = new BranchNode();
        }
    }
}
