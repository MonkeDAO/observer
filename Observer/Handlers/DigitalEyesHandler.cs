using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;

namespace Observer.Handlers
{
    public class DigitalEyesHandler : Handler, IHandler
    {
        private CollectionProvider _collectionProvider;
        
        public DigitalEyesHandler(string name, string address)
            : base(name, address){ }

        /// <inheritdoc cref="IHandler.Handle"/>
        public async void Handle(TransactionMetaInfo tx)
        {
            _logger.LogDebug($"{_name} - Handling transaction - {tx.Transaction.Signatures[0]}");
            var decodedInstructions = InstructionDecoder.DecodeInstructions(tx);

            switch (decodedInstructions.Count)
            {
                case 1:
                    // this is a delisting
                    if (decodedInstructions[0].InnerInstructions.Count == 2)
                    {
                        var tokenAccountDestination = (PublicKey)decodedInstructions[0].InnerInstructions[0].Values
                            .GetValueOrDefault("Destination");
                        var tokenAccountInfo = await MetaplexHelpers.GetAccountInfo(_client, tokenAccountDestination);
                        var (nftMint, actualOwner) = MetaplexHelpers.GetTokenAccountMintAndOwner(tokenAccountInfo);
                        var metadataAccount = _collectionProvider.GetMetadataAccountForMint(nftMint);
                        /**/
                        if (metadataAccount != null)
                        {
                             
                            _logger.LogInformation(
                                $"[{_name}] UNLISTED → {actualOwner?.Key[..5]}...{actualOwner?.Key[^5..]}" +
                                $" REMOVED LISTING FOR {metadataAccount.Name}" +
                                $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                            break;
                        }
                        _logger.LogInformation($"[{_name}] UNLISTED → {actualOwner?.Key[..5]}...{actualOwner?.Key[^5..]}" +
                                               $" REMOVED LISTING FOR {nftMint?.Key[..5]}...{nftMint?.Key[^5..]}" +
                                               $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                    }
                    break;
                case 2:
                    // this is a sale
                    if (decodedInstructions[0].InnerInstructions.Count == 4)
                    {
                        var from = (PublicKey) decodedInstructions[1].InnerInstructions[0].Values.GetValueOrDefault("From Account");
                        var to = (PublicKey) decodedInstructions[1].InnerInstructions[2].Values.GetValueOrDefault("To Account");
                        var feeAmount = (ulong) decodedInstructions[1].InnerInstructions[0].Values.GetValueOrDefault("Amount");
                        var nftMint = (PublicKey) decodedInstructions[0].InnerInstructions[3].Values.GetValueOrDefault("Mint");
                        if (feeAmount == null || from == null || to == null || nftMint == null) break;

                        var amount = decodedInstructions[1].InnerInstructions
                            .Select(
                                instruction => 
                                    instruction.Values.GetValueOrDefault("Amount"))
                            .Where(innerAmount => innerAmount != null)
                            .Aggregate(0UL, (current, innerAmount) => current + (ulong)innerAmount);

                        var metadataAccount = _collectionProvider.GetMetadataAccountForMint(nftMint);
                        var price = (double) ((amount + feeAmount) / MetaplexHelpers.LamportsPerSol);
                    
                        if (metadataAccount != null)
                        {
                            _logger.LogInformation($"[{_name}] SOLD → {from?.Key[..5]}...{from?.Key[^5..]}" +
                                                   $" BOUGHT {metadataAccount.Name}" +
                                                   $" FROM {to?.Key[..5]}...{to?.Key[^5..]}" +
                                                   $" FOR {price} SOL" +
                                                   $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                            break;
                        }
                        _logger.LogInformation($"[{_name}] SOLD → {from?.Key[..5]}...{from?.Key[^5..]}" +
                                               $" BOUGHT {nftMint?.Key[..5]}...{nftMint?.Key[^5..]}" +
                                               $" FROM {to?.Key[..5]}...{to?.Key[^5..]}" +
                                               $" FOR {price:N2} SOL" +
                                               $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                    }
                    break;
                case 5:
                    // this is a listing
                    if (decodedInstructions[4].InnerInstructions.Count == 2)
                    {
                        var from = (PublicKey) decodedInstructions[0].Values.GetValueOrDefault("Owner Account");
                        var nftMint = (PublicKey) decodedInstructions[1].Values.GetValueOrDefault("Mint");
                        var data = (string) decodedInstructions[4].Values.GetValueOrDefault("Data");
                        if (from == null || data == null || nftMint == null) break;

                        var price = (double)(MetaplexHelpers.GetPriceFromData(Encoders.Base58.DecodeData(data)) /
                                             MetaplexHelpers.LamportsPerSol);
                        var metadataAccount = _collectionProvider.GetMetadataAccountForMint(nftMint);
                        /**/
                        if (metadataAccount != null)
                        {
                            _logger.LogInformation($"[{_name}] LISTED → {from?.Key[..5]}...{from?.Key[^5..]}" +
                                                   $" LISTED {metadataAccount.Name}" +
                                                   $" FOR {(price * 1.053):N2} SOL" +
                                                   $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                            break;
                        }
                        _logger.LogInformation($"[{_name}] LISTED → {from.Key[..5]}...{from.Key[^5..]}" +
                                               $" LISTED {nftMint?.Key[..5]}...{nftMint?.Key[^5..]}" +
                                               $" FOR {(price * 1.053):N2} SOL" +
                                               $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                    }
                    break;
            }
        }

        /// <inheritdoc cref="IHandler.SetProvider"/>
        public void SetProvider(CollectionProvider provider)
        {
            _collectionProvider = provider;
        }

        /// <inheritdoc cref="IHandler.SetLogger"/>
        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc cref="IHandler.SetClient"/>
        public void SetClient(IRpcClient client)
        {
            _client = client;
        }
    }
}