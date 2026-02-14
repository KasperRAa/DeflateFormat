using DeflateFormat.Huffmans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat
{
    /// <summary>
    /// A class for compressing and decompressing a byte[] into another byte[] via the Deflate formate
    /// </summary>
    public class Deflate
    {
        #region Fields
        private int _maxLength;
        private int _maxDistance;
        private int? _maxBlockLength;
        #endregion

        #region Properties
        /// <summary>
        /// The method used for compression (Optimal, Dynamic, Static, Raw)
        /// </summary>
        public CompressionMethod Method { get; set; }

        /// <summary>
        /// The max length the compressor will repeate a sequence. Must be within (3, 258)
        /// </summary>
        public int MaxLength
        {
            get
            {
                return _maxLength;
            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 258, "MaxLength");
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 3, "MaxLength");
                _maxLength = value;
            }
        }

        /// <summary>
        /// The max distance the compressor will look back for repetitions of sequences. Must be within (1, 32768)
        /// </summary>
        public int MaxDistance
        {
            get
            {
                return _maxDistance;
            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 32768, "MaxDistance");
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1, "MaxDistance");
                _maxDistance = value;
            }
        }

        /// <summary>
        /// The max length of each block (only the last block will be shorter). Must be at least 1, or null for 1 block.
        /// </summary>
        public int? MaxBlockLength
        {
            get
            {
                return _maxBlockLength;
            }
            set
            {
                if (value != null) ArgumentOutOfRangeException.ThrowIfLessThan((int)value, 1, "MaxBlockLength");
                _maxBlockLength = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor. Sets all variables to their most optimised values.
        /// </summary>
        public Deflate() : this(CompressionMethod.Optimal, 258, 32768, null) { }

        /// <summary>
        /// Constructor for setting the CompressionMethod. Sets all other variables to their most optimised values.
        /// </summary>
        /// <param name="method">The method used for compression (Optimal, Dynamic, Static, Raw).</param>
        public Deflate(CompressionMethod method) : this(method, 258, 32768, null) { }

        /// <summary>
        /// Constructor for setting the MaxLength and MaxDistance. Sets all other variables to their most optimised values.
        /// </summary>
        /// <param name="maxLength">The max distance the compressor will look back for repetitions of sequences. Must be within (1, 32768).</param>
        /// <param name="maxDistance">The max length of each block (only the last block will be shorter). Must be at least 32768, or null for 1 block.</param>
        public Deflate(int maxLength, int maxDistance) : this(CompressionMethod.Optimal, maxLength, maxDistance, null) { }

        /// <summary>
        /// Constructor for setting the MaxBlockLength. Sets all other variables to their most optimised values.
        /// </summary>
        /// <param name="maxBlockLength">The max length of each block (only the last block will be shorter). Must be at least 32768, or null for 1 block.</param>
        public Deflate(int? maxBlockLength) : this(CompressionMethod.Optimal, 258, 32768, maxBlockLength) { }

        /// <summary>
        /// Constructor for setting all values.
        /// </summary>
        /// <param name="method">The method used for compression (Optimal, Dynamic, Static, Raw).</param>
        /// <param name="maxLength">The max distance the compressor will look back for repetitions of sequences. Must be within (1, 32768).</param>
        /// <param name="maxDistance">The max length of each block (only the last block will be shorter). Must be at least 32768, or null for 1 block.</param>
        /// <param name="maxBlockLength">The max length of each block (only the last block will be shorter). Must be at least 32768, or null for 1 block.</param>
        public Deflate(CompressionMethod method, int maxLength, int maxDistance, int? maxBlockLength)
        {
            Method = method;
            MaxLength = maxLength;
            MaxDistance = maxDistance;
            MaxBlockLength = maxBlockLength;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Compresses an array of bytes via the Deflate format, using the class's settings.
        /// </summary>
        /// <param name="input">Uncompressed bytes</param>
        /// <returns>Compressed bytes</returns>
        public byte[] Compress(byte[] input)
        {
            //Deal with an empty input
            if (input.Length == 0) return Array.Empty<byte>();

            List<byte> result = new List<byte>();
            int position = 0;
            if (MaxBlockLength != null) throw new NotImplementedException("Multiple Blocks");
            else DeflateReadWrite.WriteBit(result, ref position, true);
            
            switch (Method)
            {
                case CompressionMethod.Raw:
                    CompressRaw(result, input, ref position);
                    break;
                case CompressionMethod.Static:
                    CompressStatic(result, input, ref position);
                    break;
                case CompressionMethod.Dynamic:
                    CompressDynamic(result, input, ref position);
                    break;
                default:
                    throw new NotImplementedException("Optimal");
            }
            return result.ToArray();
        }

        /// <summary>
        /// Deompresses an array of bytes compressed via the Deflate format.
        /// </summary>
        /// <param name="input">Compressed bytes</param>
        /// <returns>Uncompressed bytes</returns>
        public byte[] Decompress(byte[] input)
        {
            //Deal with an empty input
            if (input.Length == 0) return Array.Empty<byte>();

            int position = 0;

            bool isFinalBlock = DeflateReadWrite.ReadBit(input, ref position);
            if (!isFinalBlock) throw new NotImplementedException("Multiple Blocks");

            switch (DeflateReadWrite.ReadInt(input, ref position, 2))
            {
                case 0:
                    return DecompressRaw(input, ref position);
                case 1:
                    return DecompressStatic(input, ref position);
                case 2:
                    return DecompressDynamic(input, ref position);
                default:
                    throw new FormatException("Reserved Compression Method");
            }
        }
        #endregion

        #region Private Methods
        #region Compression
        private void CompressRaw(List<byte> result, byte[] input, ref int position)
        {
            DeflateReadWrite.WriteInt(result, ref position, 0, 2);
            while (position % 8 != 0) position++;
            int length = input.Length;
            DeflateReadWrite.WriteInt(result, ref position, length, 16);
            DeflateReadWrite.WriteInt(result, ref position, ~length, 16);
            for (int i = 0; i < length; i++) result.Add(input[i]);
        }
        private void CompressStatic(List<byte> result, byte[] input, ref int position)
        {
            DeflateReadWrite.WriteInt(result, ref position, 1, 2);

            CodeSequence codeSequence = CodeSequence.Encode(input, MaxLength, MaxDistance);

            DeflateHuffman huffman = DeflateHuffman.GetStatic();

            huffman.Write(result, ref position, codeSequence);
        }
        private void CompressDynamic(List<byte> result, byte[] input, ref int position)
        {
            DeflateReadWrite.WriteInt(result, ref position, 2, 2);
            throw new NotImplementedException("Dynamic");
        }
        #endregion
        #region Decompression
        private byte[] DecompressRaw(byte[] bytes, ref int position)
        {
            while (position % 8 != 0) position++;

            int length = DeflateReadWrite.ReadInt(bytes, ref position, 16);
            int invLength = DeflateReadWrite.ReadInt(bytes, ref position, 16);
            if (length != (~invLength & 0b1111111111111111)) throw new FormatException();
            List<byte> result = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                result.Add(bytes[position / 8]);
                position += 8;
            }
            return result.ToArray();
        }
        private byte[] DecompressStatic(byte[] bytes, ref int position)
        {
            DeflateHuffman huffman = DeflateHuffman.GetStatic();

            CodeSequence codeSequence = huffman.Read(bytes, ref position);

            byte[] result = codeSequence.Decode();

            return result;
        }
        private byte[] DecompressDynamic(byte[] bytes, ref int position)
        {
            throw new NotImplementedException();
        }
        #endregion
        #endregion
    }
}
