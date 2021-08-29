using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Solnet.Rpc;

namespace Scraper
{
    class Program
    {
        /// <summary>
        /// The rpc client instance.
        /// </summary>
        private static IRpcClient _rpcClient;
        
        /// <summary>
        /// The names of the collections to get and the separator of their number.
        /// </summary>
        private static Dictionary<string, string> _collections = new()
        {
            /*{"Bold Badger", "#"}
            { "SolPunk", "#" },
            { "Degen Ape", "#"}, 
            { "Solanimal", "#" },
            { "SolMee", "#"},
            { "THUG", "#" },
            { "Top Hat Chicks", "#" },
            { "Solstones", ""},
            { "Nogoalfaces", ""},
            { "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "#"},
            { "BOOGLE", "#" },
            { "SSBxSolPunk", "Punk_#" },
            { "Abstraction", "No."},
            { "Frakt", "-"},           
            { "SolanaDoge", "#" },
            { "Sollamas", "#" },
            { "SMB", "#"}*/
        };

        private static void Main(string[] args)
        {
            ILogger _logger = null;
#if DEBUG
            _logger = GetDebugLogger();
#elif RELEASE
            _logger = GetInformationLogger();
#endif
            _rpcClient = ClientFactory.GetClient(Cluster.MainNet);
            
            _logger.LogInformation($"Launching scrapers.");

            foreach (var collection in _collections)
            {
                if (collection.Key == "SMB")
                {
                     var symbolScraper = new SymbolScraper(
                         _rpcClient, 
                         _logger,
                         collection.Key,
                         collection.Value);
                     Task.Run(() => symbolScraper.Run());
                     continue;
                }
                var nameScraper = new NameScraper(
                    _rpcClient, 
                    _logger,
                    collection.Key,
                    collection.Value);
                Task.Run(() => nameScraper.Run());
            }
            
            _logger.LogInformation("Finished scraping metadata.");
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
            }).CreateLogger<NameScraper>();
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
            }).CreateLogger<NameScraper>();
        }
    }
}