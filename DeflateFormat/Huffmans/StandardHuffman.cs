using DeflateFormat.Codes;
using DeflateFormat.Huffmans.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat.Huffmans
{
    internal class StandardHuffman
    {
        private HuffmanNode _seed;
        private string[] _sequences;

        public StandardHuffman(string[] sequences)
        {
            _sequences = sequences;

            _seed = new HuffmanNode();
            int length = sequences.Length;
            if (length == 1) throw new NotImplementedException();

            for (int i = 0; i < length; i++)
            {
                BranchNode lastNode = new BranchNode();
                BranchNode currentNode = (BranchNode)_seed.Node;

                string sequence = sequences[i];
                foreach (char c in sequence)
                {
                    lastNode = currentNode;
                    if (c == '0')
                    {
                        if (currentNode.Left.GetType() == typeof(Node)) currentNode.Left = new BranchNode();
                        currentNode = (BranchNode)currentNode.Left;
                    }
                    else if (c == '1')
                    {
                        if (currentNode.Right.GetType() == typeof(Node)) currentNode.Right = new BranchNode();
                        currentNode = (BranchNode)currentNode.Right;
                    }
                    else throw new Exception($"Invalid sequence for Huffman {{{c}}}");
                }

                if (sequence.Last() == '0') lastNode.Left = new LeafNode(i);
                else lastNode.Right = new LeafNode(i);
            }

            ConfirmHuffman(_seed.Node);
        }

        private void ConfirmHuffman(Node node, string sequence = "")
        {
            switch (node)
            {
                case BranchNode:
                    ConfirmHuffman(((BranchNode)node).Left, sequence + '0');
                    ConfirmHuffman(((BranchNode)node).Right, sequence + '1');
                    break;
                case LeafNode:
                    break;
                case Node:
                    throw new Exception($"Huffman was formed incorrectly {{{sequence}}}");
            }
        }

        public int Read(IReadOnlyList<byte> bytes, ref int position)
        {
            if (_seed.Node.GetType() != typeof(BranchNode)) throw new NotImplementedException("Single-Value-Huffman");

            Node node = _seed.Node;
            while (node.GetType() == typeof(BranchNode))
            {
                BranchNode branch = (BranchNode)node;
                node = DeflateReadWrite.ReadBit(bytes, ref position) ? branch.Right : branch.Left;
            }

            return ((LeafNode)node).Value;
        }

        public void Write(List<byte> bytes, ref int position, int value)
        {
            string sequence = _sequences[value];
            foreach (char c in sequence) DeflateReadWrite.WriteBit(bytes, ref position, c == '1');
        }
    }
}
