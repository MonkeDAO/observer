using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Rpc.Utilities;
using Solnet.Wallet;

namespace Observer
{
    /// <summary>
    /// Implements helpers for calculations related to Metaplex protocol.
    /// </summary>
    public static class MetaplexHelpers
    {
        /// <summary>
        /// The prefix bytes for the metadata account PDA.
        /// </summary>
        private static readonly byte[] PrefixBytes = Encoding.UTF8.GetBytes("PREFIX");
        
        /// <summary>
        /// The Metaplex program.
        /// </summary>
        public static PublicKey ProgramId = new ("metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s");
        
        /// <summary>
        /// Number of lamports per sol, used to calculate SOL values.
        /// </summary>
        public const int LamportsPerSol = 1_000_000_000;

        /// <summary>
        /// Gets price value from instruction data for SMB listings.
        /// </summary>
        /// <param name="data">The instruction data.</param>
        /// <returns></returns>
        public static ulong GetPriceFromData(ReadOnlySpan<byte> data)
        {
            var price = data.GetU64(1);
            return price;
        }
        
        public static async Task<AccountInfo> GetAccountInfo(IRpcClient client, PublicKey address)
        {
            while (true)
            {
                var tokenAccountInfo = await client.GetAccountInfoAsync(address);

                if (tokenAccountInfo.WasRequestSuccessfullyHandled) return tokenAccountInfo.Result.Value;
                
                await Task.Delay(250);
            }
        }

        public static (PublicKey mint, PublicKey owner) GetTokenAccountMintAndOwner(AccountInfo tokenAccountInfo)
        {
            var accountData = (ReadOnlySpan<byte>) Convert.FromBase64String(tokenAccountInfo.Data[0]).AsSpan();
                        
            var nftMint = accountData.GetPubKey(0);
            var actualOwner = accountData.GetPubKey(32);

            return (nftMint, actualOwner);
        }
        
        public static PublicKey DeriveMetadataAccount(PublicKey mint)
        {
            var success = AddressExtensions.TryFindProgramAddress(
                new List<byte[]>() { PrefixBytes, ProgramId, mint }, ProgramId,
                out var address, out _);

            return success ? new PublicKey(address) : null;
        }
    }
}