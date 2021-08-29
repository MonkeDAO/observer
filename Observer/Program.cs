using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Solnet.Rpc;

namespace Observer
{
    internal static class Program
    {
        private static IRpcClient RpcClient;
        private static List<MarketplaceWatcher> Watchers;
        private static List<CancellationTokenSource> CancellationTokenSources;

        private static void Main(string[] args)
        {
            ILogger logger = null;
#if DEBUG
            logger = GetDebugLogger();
#elif RELEASE
            logger = GetInformationLogger();
#endif
            
            RpcClient = ClientFactory.GetClient(Cluster.MainNet);
            Watchers = new List<MarketplaceWatcher>(Marketplaces.AddressMap.Count);
            CancellationTokenSources = new List<CancellationTokenSource>(Marketplaces.AddressMap.Count);
            logger.LogInformation($"Creating collection provider.");

            var cp = new CollectionProvider(logger);
            cp.LoadMetadata().Wait();
            
            logger.LogInformation($"Launching {Marketplaces.AddressMap.Count} watchers.");
            foreach (var (key, value) in Marketplaces.AddressMap)
            {
                var cts = new CancellationTokenSource();
                var watcher = new MarketplaceWatcher(logger, RpcClient, cts.Token, key, value);
                
                var handlerExists = Marketplaces.HandlerMap.TryGetValue(key, out var handler);
                if (!handlerExists) throw new Exception($"no handler for {key} found");
                handler.SetLogger(logger);
                handler.SetClient(RpcClient);
                handler.SetProvider(cp);
                
                Task.Run(async () =>
                {
                    await watcher.Run(handler);
                }, cts.Token);
                logger.LogInformation($"Launched watcher for {key} - {value}.");
                
                Watchers.Add(watcher);
                CancellationTokenSources.Add(cts);
            }

            Console.ReadKey();

            logger.LogInformation($"Stopping {Marketplaces.AddressMap.Count} watchers.");
            foreach (var cts in CancellationTokenSources)
            {
                cts.Cancel();
            }
            
            Console.ReadKey();
        }
        
        private static ILogger GetInformationLogger()
        {
            return LoggerFactory.Create(x =>
            {
                x.AddSimpleConsole(o =>
                    {
                        o.UseUtcTimestamp = true;
                        o.IncludeScopes = true;
                        o.ColorBehavior = LoggerColorBehavior.Enabled;
                        o.TimestampFormat = "HH:mm:ss ";
                    })
                    .SetMinimumLevel(LogLevel.Information);
            }).CreateLogger<MarketplaceWatcher>();
        }

        private static ILogger GetDebugLogger()
        {
            return LoggerFactory.Create(x =>
            {
                x.AddSimpleConsole(o =>
                    {
                        o.UseUtcTimestamp = true;
                        o.IncludeScopes = true;
                        o.ColorBehavior = LoggerColorBehavior.Enabled;
                        o.TimestampFormat = "HH:mm:ss ";
                    })
                    .SetMinimumLevel(LogLevel.Debug);
            }).CreateLogger<MarketplaceWatcher>();
        }
    }
}