using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Observer.Handlers;
using Solnet.Rpc;
using Solnet.Rpc.Types;

namespace Observer
{
    /// <summary>
    /// Implements a program watcher.
    /// </summary>
    public class MarketplaceWatcher
    {
        /// <summary>
        /// The rpc client instance.
        /// </summary>
        private IRpcClient _rpcClient;
        
        /// <summary>
        /// The logger instance.
        /// </summary>
        private ILogger _logger;
        
        /// <summary>
        /// The name of the NFT this watcher is tracking.
        /// </summary>
        private string _name;

        /// <summary>
        /// The address of the program to track.
        /// </summary>
        private string _programAddress;
        
        /// <summary>
        /// The cancellation token to stop this watcher's working loop.
        /// </summary>
        private CancellationToken _cancellationToken;

        /// <summary>
        /// The signature of the most recent transaction.
        /// </summary>
        private string _lastHash;

        /// <summary>
        /// Initialize the watcher for the given program.
        /// </summary>
        /// <param name="rpcClient">The rpc client instance.</param>
        /// <param name="cancellationToken">A cancellation token to stop the watcher.</param>
        /// <param name="programName">The name of the NFT to watch.</param>
        /// <param name="programAddress">The NFT program's address.</param>
        public MarketplaceWatcher(ILogger logger, IRpcClient rpcClient, CancellationToken cancellationToken, string programName, string programAddress)
        {
            _logger = logger;
            _rpcClient = rpcClient;
            _name = programName;
            _programAddress = programAddress;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Run the watcher.
        /// </summary>
        public async Task Run(IHandler handler)
        {
            var signatures = await _rpcClient.GetConfirmedSignaturesForAddress2Async(_programAddress, 1, commitment: Commitment.Confirmed);
            _lastHash = signatures.Result[0].Signature;
            _logger.LogInformation($"[{_name}] Latest signature {_lastHash}.");
            
            while (!_cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"[{_name}] Watching - Latest signature {_lastHash}.");
                
                signatures = await _rpcClient.GetConfirmedSignaturesForAddress2Async(_programAddress, until: _lastHash, commitment: Commitment.Confirmed);
                if (!signatures.WasRequestSuccessfullyHandled)
                {
                    await Task.Delay(1000, _cancellationToken);
                    continue;
                }
                
                if(signatures.Result.Count == 0)
                {
                    _logger.LogDebug($"[{_name}] No new signatures found - Watcher sleeping - Latest signature {_lastHash}.");
                    await Task.Delay(10000, _cancellationToken);
                    continue;
                }

                _logger.LogDebug($"[{_name}] {signatures.Result.Count} new signatures found.");

                for(var i = signatures.Result.Count - 1; i >= 0; i--)
                {
                    var tx = await _rpcClient.GetConfirmedTransactionAsync(signatures.Result[i].Signature, Commitment.Confirmed);
                    if (!tx.WasRequestSuccessfullyHandled)
                    {
                        await Task.Delay(1000, _cancellationToken);
                        continue;
                    }
                    if (!tx.Result.BlockTime.HasValue)
                    {
                        await Task.Delay(1000, _cancellationToken);
                        continue;
                    }
                    if (DateTime.UtcNow.Subtract(DateTime.UnixEpoch.Add(TimeSpan.FromSeconds(tx.Result.BlockTime.Value))).TotalSeconds > 300)
                    {
                        _logger.LogDebug($"[{_name}] Found old transaction, ignoring.");
                        await Task.Delay(1000, _cancellationToken);
                        continue;
                    }
                    
                    if (tx.Result.Meta.Error != null || tx.Result.Meta.LogMessages.Any(x=> x.Contains("failed")))
                    {
                        await Task.Delay(1000, _cancellationToken);
                        continue;
                    }

                    handler.Handle(tx.Result);

                    await Task.Delay(1000, _cancellationToken);
                    if (i != 0) continue;
                    
                    _lastHash = signatures.Result[i].Signature;
                    break;
                }
                _logger.LogDebug($"[{_name}] Watcher sleeping - Latest signature {_lastHash}.");
                await Task.Delay(10000, _cancellationToken);
            }
            _logger.LogInformation($"[{_name}] Watcher stopped.");
        }
    }
}