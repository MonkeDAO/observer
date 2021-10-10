using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;

namespace Observer.Handlers
{
    /// <summary>
    /// Implements an handler for Solana Monkey Business NFTs.
    /// </summary>
    public class SolanartHandler : Handler, IHandler
    {
        private CollectionProvider _collectionProvider;

        public SolanartHandler(string name, string address)
            : base(name, address) { }
        
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
                        var from = (PublicKey) decodedInstructions[1].InnerInstructions[1].Values.GetValueOrDefault("From Account");
                        var to = (PublicKey) decodedInstructions[1].InnerInstructions[1].Values.GetValueOrDefault("To Account");
                        var feeAmount = decodedInstructions[1].InnerInstructions[0].Values.GetValueOrDefault("Amount");
                        var amount = decodedInstructions[1].InnerInstructions[1].Values.GetValueOrDefault("Amount");
                        var nftMint = (PublicKey) decodedInstructions[0].InnerInstructions[3].Values.GetValueOrDefault("Mint");
                        if (amount == null || feeAmount == null || from == null || to == null || nftMint == null) break;
                        var metadataAccount = _collectionProvider.GetMetadataAccountForMint(nftMint);
                        var price = (double) ( 100 * ((ulong) amount + (ulong) feeAmount) / MetaplexHelpers.LamportsPerSol) / 100;
                    
                        if (metadataAccount != null)
                        {
                            _logger.LogInformation($"[{_name}] SOLD → {from.Key[..5]}...{from.Key[^5..]}" +
                                                   $" BOUGHT {metadataAccount.Name}" +
                                                   $" FROM {to.Key[..5]}...{to.Key[^5..]}" +
                                                   $" FOR {price:N2} SOL" +
                                                   $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                            break;
                        }
                        _logger.LogInformation($"[{_name}] SOLD → {from.Key[..5]}...{from.Key[^5..]}" +
                                               $" BOUGHT {nftMint?.Key[..5]}...{nftMint?.Key[^5..]}" +
                                               $" FROM {to.Key[..5]}...{to.Key[^5..]}" +
                                               $" FOR {price:N2} SOL" +
                                               $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                    }
                    break;
                case 5:
                    // this is a listing
                    if (decodedInstructions[3].InnerInstructions.Count == 5)
                    {
                        var from = (PublicKey)decodedInstructions[0].Values.GetValueOrDefault("Owner Account");
                        var nftMint = (PublicKey)decodedInstructions[1].Values.GetValueOrDefault("Mint");
                        var data = (string)decodedInstructions[4].Values.GetValueOrDefault("Data");
                        if (from == null || data == null || nftMint == null) break;

                        var metadataAccount = _collectionProvider.GetMetadataAccountForMint(nftMint);
                        var jsonMemo = Encoders.Base58.DecodeData(data);
                        var jsonDoc = JsonDocument.Parse(jsonMemo);
                        var priceStr = jsonDoc.RootElement.GetProperty("price_sol").GetString();
                        priceStr = priceStr?.Trim('"');
                        var price = 0f;
                        if (priceStr != null)
                            price = float.Parse(priceStr);
                        /**/
                        if (metadataAccount != null)
                        {
                            
                            _logger.LogInformation($"[{_name}] LISTED → {from.Key[..5]}...{from.Key[^5..]}" +
                                                   $" LISTED {metadataAccount.Name}" +
                                                   $" FOR {price:N2} SOL" +
                                                   $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                            break;
                        }
                        _logger.LogInformation($"[{_name}] LISTED → {from.Key[..5]}...{from.Key[^5..]}" +
                                               $" LISTED {nftMint?.Key[..5]}...{nftMint?.Key[^5..]}" +
                                               $" FOR {price:N2} SOL" +
                                               $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                    } else if (decodedInstructions[0].InnerInstructions.Count == 4)
                    { // this is a sale
                        HandleSale(tx, decodedInstructions);
                    }
                    break;
                default:
                {
                    try
                    {
                        HandleSale(tx, decodedInstructions);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation(
                            $"[{_name}] Irregular transaction: {tx.Transaction.Signatures[0]}. Exception: {ex}");
                    }
                    break;
                }
            }
        }
        
        private async void HandleSale(TransactionMetaInfo tx, List<DecodedInstruction> decodedInstructions)
        {
            var from = (PublicKey) decodedInstructions[1].InnerInstructions[1].Values.GetValueOrDefault("From Account");
            var to = (PublicKey) decodedInstructions[1].InnerInstructions[1].Values.GetValueOrDefault("To Account");
            var feeAmount = decodedInstructions[1].InnerInstructions[0].Values.GetValueOrDefault("Amount");
            var amount = decodedInstructions[1].InnerInstructions[1].Values.GetValueOrDefault("Amount");
            var nftMint = (PublicKey) decodedInstructions[0].InnerInstructions[3].Values.GetValueOrDefault("Mint");
            if (amount == null || feeAmount == null || from == null || to == null || nftMint == null) return;
            var metadataAccount = _collectionProvider.GetMetadataAccountForMint(nftMint);
            
            var otherAmounts = decodedInstructions
                .Select(
                    instruction => 
                        instruction.Values.GetValueOrDefault("Amount"))
                .Where(innerAmount => innerAmount != null)
                .Aggregate(0UL, (current, innerAmount) => current + (ulong)innerAmount);
            
            var price = (double) (100 * ((ulong) amount + (ulong) feeAmount + otherAmounts)/ MetaplexHelpers.LamportsPerSol) / 100;
        
            if (metadataAccount != null)
            {
                _logger.LogInformation($"[{_name}] SOLD → {from.Key[..5]}...{from.Key[^5..]}" +
                                       $" BOUGHT {metadataAccount.Name}" +
                                       $" FROM {to.Key[..5]}...{to.Key[^5..]}" +
                                       $" FOR {price:N2} SOL" +
                                       $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
                return;
            }
            _logger.LogInformation($"[{_name}] SOLD → {from.Key[..5]}...{from.Key[^5..]}" +
                                   $" BOUGHT {nftMint?.Key[..5]}...{nftMint?.Key[^5..]}" +
                                   $" FROM {to.Key[..5]}...{to.Key[^5..]}" +
                                   $" FOR {price:N2} SOL" +
                                   $" → https://solscan.io/tx/{tx.Transaction.Signatures[0]}");
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
