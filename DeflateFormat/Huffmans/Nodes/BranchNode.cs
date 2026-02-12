using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat.Huffmans.Nodes
{
    internal class BranchNode : Node
    {
        public Node Left;
        public Node Right;

        public BranchNode()
        {
            Left = new Node();
            Right = new Node();
        }
    }
}
