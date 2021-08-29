using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Observer.Models;
using Solnet.Rpc;

namespace Scraper
{
    public class SymbolScraper : ScraperBase
    {
                
        /// <summary>
        /// Initialize the name scraper.
        /// </summary>
        /// <param name="client">The rpc client instance.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="name">The name to search for.</param>
        /// <param name="separator">The separator for the non-fungible token id.</param>
        public SymbolScraper(IRpcClient client, ILogger logger, string name, string separator) : base(client, logger, name, separator)
        {
            _name = name;
            _sep = separator;
        }

        /// <summary>
        /// Run the scraper to build datasets of metadata.
        /// </summary>
        public async Task Run()
        {
            _logger.LogInformation($"Loading metadata accounts for {_name}.");
            // 105 is the offset of the symbol in the metadata account
            var metadataAccounts = await GetMetadataAccounts(105, _name);
            _metadataAccountWrappers = new List<MetadataAccountWrapper>();
            foreach (var metadataAccount in metadataAccounts)
            {
                var metadata =
                    MetadataAccount.Deserialize(Convert.FromBase64String(metadataAccount.Account.Data[0]));
                /*
                _logger.LogInformation($"Decoded Data Account: {metadataAccount.PublicKey}\t" +
                                       $"- Mint: {metadata.Mint}\t" +
                                       $"- Name: {metadata.Data.Name}\t" +
                                       $"- Symbol: {metadata.Data.Symbol}\t" +
                                       $"- Uri: {metadata.Data.Uri}");
                                       */
                
                metadata.Data.Name = metadata.Data.Name.Trim('\0');
                metadata.Data.Symbol = metadata.Data.Symbol.Trim('\0');
                metadata.Data.Uri = metadata.Data.Uri.Trim('\0');
                if (string.IsNullOrWhiteSpace(metadata.Data.Uri)) continue;
                
                var idSplit = metadata.Data.Name.Split(_sep);
                var id = -1;
                if (idSplit.Length == 2)
                {
                    var success = int.TryParse(idSplit[1], out id);
                    if (!success)
                    {
                        idSplit = idSplit[1].Split(" ");
                        if (idSplit.Length != 0)
                            int.TryParse(idSplit[0], out id);
                    }
                }

                var imageUrlSplit = metadata.Data.Uri.Split(".net/");
                var imageUrl = "";
                if (imageUrlSplit.Length == 2) imageUrl = imageUrlSplit[1];
                _metadataAccountWrappers.Add(new MetadataAccountWrapper
                {
                    Address = metadataAccount.PublicKey,
                    Metadata = metadata,
                    Id = id,
                    Image = imageUrl,
                    Mint = metadata.Mint,
                    Name = metadata.Data.Name,
                    Symbol = metadata.Data.Symbol,
                    ArweaveUri = metadata.Data.Uri,
                });
            }
            
            await File.WriteAllTextAsync($"{_name.Replace(" ", "")}-metadata.json", JsonSerializer.Serialize(_metadataAccountWrappers, _jsonSerializerOptions));

            _logger.LogInformation($"[{_name}] Finished loading {_metadataAccountWrappers.Count} metadata accounts for {_name}, loading arweave.");
            var metadataWrappers = _metadataAccountWrappers.Where(item => item.ArweaveUri != null).ToList();
            var newWrappers = new List<MetadataAccountWrapper>();
            var i = 1;
            foreach (var item in metadataWrappers)
            {
                var processedItem = await GetArweaveData(item);
                if (processedItem == null) continue;
                processedItem.Mint = item.Mint;
                processedItem.Address = item.Address;
                processedItem.ArweaveUri = item.ArweaveUri;
                newWrappers.Add(processedItem);
                _logger.LogInformation($"[{_name}] [{i}/{metadataWrappers.Count}] Loaded arweave data for {item.Name} - {item.Id}.");
                i++;
            }
            
            _logger.LogInformation($"[{_name}] Finished loading arweave data, writing to file.");
            await File.WriteAllTextAsync($"{_name.Replace(" ", "")}.json", JsonSerializer.Serialize(newWrappers.Where(x => x != null).ToList(), _jsonSerializerOptions));
            
            _logger.LogInformation($"[{_name}] Finished writing metadata accounts for {_name} to file");
        }
    }
}