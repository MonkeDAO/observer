using System;
using Solnet.Programs.Utilities;

namespace Observer.Models
{
    /// <summary>
    /// Represents the actual metadata present in the metadata account.
    /// </summary>
    public class Data
    {
        /// <summary>
        /// The layout of the metadata.
        /// </summary>
        internal static class Layout
        {
            /// <summary>
            /// The offset at which the name value of the non-fungible token begins.
            /// </summary>
            internal const int NameOffset = 0;
        }

        /// <summary>
        /// The name of the non-fungible token.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The symbol of the non-fungible token collection.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The uri to the data hosted on Arweave.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// The seller fee.
        /// </summary>
        public ushort SellerFeeBasisPoints { get; set; }

        /// <summary>
        /// Deserialize the actual metadata in the metadata account into an object.
        /// <remarks>
        /// This supposes that the given byte data has been sliced and that the name starts at offset 0.
        /// </remarks>
        /// </summary>
        /// <param name="data">The raw account data.</param>
        /// <returns>The <see cref="Data"/> instance.</returns>
        public static Data Deserialize(ReadOnlySpan<byte> data)
        {
            (string name, int nameLength) = data.DecodeRustString(Layout.NameOffset);
            (string symbol, int symbolLength) = data.DecodeRustString(Layout.NameOffset + nameLength);
            (string uri, int uriLength) = data.DecodeRustString(Layout.NameOffset + nameLength + symbolLength);
            
            return new Data
            {
                Name = name,
                Symbol = symbol,
                Uri = uri,
                SellerFeeBasisPoints = data.GetU16(Layout.NameOffset + nameLength + symbolLength + uriLength),
            };
        }
    }
}