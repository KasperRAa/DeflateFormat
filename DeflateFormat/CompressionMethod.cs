using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateFormat
{
    /// <summary>
    /// Method used for deflate compression.
    /// </summary>
    public enum CompressionMethod
    {
        /// <summary>
        /// Optimal compression. The method used will be the one calculated to be the most optimal.
        /// </summary>
        Optimal,
        /// <summary>
        /// Dynamic compression.
        /// </summary>
        Dynamic,
        /// <summary>
        /// Static compression.
        /// </summary>
        Static,
        /// <summary>
        /// Raw compression.
        /// Each block, except the last, will have a length of 65535 bytes.
        /// </summary>
        Raw
    }
}
