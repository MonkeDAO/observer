using System;
using Solnet.Programs.Utilities;
using Solnet.Wallet;

namespace Observer.Models
{
    /// <summary>
    /// Represents a metadata account.
    /// </summary>
    public class MetadataAccount
    {
        /// <summary>
        /// The layout of the metadata account.
        /// </summary>
        private static class Layout
        {
            /// <summary>
            /// The offset for the key value begins.
            /// </summary>
            internal const int KeyOffset = 0;

            /// <summary>
            /// The offset at which the update authority value begins.
            /// </summary>
            internal const int UpdateAuthorityOffset = 1;
            
            /// <summary>
            /// The offset at which the mint value begins.
            /// </summary>
            internal const int MintOffset = 33;

            /// <summary>
            /// The offset at which the <see cref="MetadataAccount.Data"/> object begins.
            /// </summary>
            internal const int MetadataOffset = 65;
        }

        /// <summary>
        /// A key which represents which type of edition this non-fungible token is.
        /// </summary>
        public byte Key { get; set; }

        /// <summary>
        /// The non-fungible token's metadata update authority.
        /// </summary>
        public PublicKey UpdateAuthority { get; set; }

        /// <summary>
        /// The mint of the non-fungible token.
        /// </summary>
        public PublicKey Mint { get; set; }

        /// <summary>
        /// The object which contains the actual metadata.
        /// </summary>
        public Data Data { get; set; }
        
        /// <summary>
        /// Deserialize the metadata account.
        /// </summary>
        /// <param name="rawData">The raw account data.</param>
        /// <returns>The <see cref="MetadataAccount"/> instance.</returns>
        public static MetadataAccount Deserialize(ReadOnlySpan<byte> rawData)
        {
            var data = Data.Deserialize(rawData[Layout.MetadataOffset..]);

            return new MetadataAccount
            {
                Key = rawData.GetU8(Layout.KeyOffset),
                UpdateAuthority = rawData.GetPubKey(Layout.UpdateAuthorityOffset),
                Mint = rawData.GetPubKey(Layout.MintOffset),
                Data = data,
            };
        }
    }
}