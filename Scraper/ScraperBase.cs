using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Observer;
using Observer.Models;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Wallet.Utilities;

namespace Scraper
{
    public abstract class ScraperBase
    {
        /// <summary>
        /// The web client instance to fetch data from arweave.
        /// </summary>
        protected WebClient _webClient;

        /// <summary>
        /// The rpc client instance.
        /// </summary>
        protected IRpcClient _client;
        
        /// <summary>
        /// The logger instance.
        /// </summary>
        protected ILogger _logger;

        /// <summary>
        /// The name of the collection.
        /// </summary>
        protected string _name;

        /// <summary>
        /// The name of the collection.
        /// </summary>
        protected string _sep;

        /// <summary>
        /// Metadata account wrappers.
        /// </summary>
        protected List<MetadataAccountWrapper> _metadataAccountWrappers;

        /// <summary>
        /// The json serializer options.
        /// </summary>
        protected JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initialize the base scraper.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        /// <param name="name"></param>
        /// <param name="separator"></param>
        public ScraperBase(IRpcClient client, ILogger logger, string name, string separator)
        {
            _client = client;
            _logger = logger;
            _name = name;
            _sep = separator;
            _webClient = new WebClient();
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }
        
        /// <summary>
        /// Gets the program accounts associated with the metaplex metadata program with the given string as the name.
        /// </summary>
        /// <param name="offset">The offset at which to compare with acocunt data.</param>
        /// <param name="name">The name of the collection.</param>
        /// <returns>The list of keys.</returns>
        protected async Task<List<AccountKeyPair>> GetMetadataAccounts(int offset, string name)
        {
            var accounts = await _client.GetProgramAccountsAsync(MetaplexHelpers.ProgramId,
                memCmpList: new List<MemCmp>
                {
                    new () { 
                        Offset = offset, 
                        Bytes = Encoders.Base58.EncodeData(Encoding.UTF8.GetBytes(name)) 
                    }
                });

            return accounts.Result;
        }
        
        /// <summary>
        /// Gets the program accounts associated with the metaplex metadata program with the given string as the name.
        /// </summary>
        /// <param name="offset">The offset at which to compare with acocunt data.</param>
        /// <param name="name">The name of the collection.</param>
        /// <returns>The list of keys.</returns>
        protected async Task<List<AccountKeyPair>> GetMetadataAccounts(string name, string symbol)
        {
            var accounts = await _client.GetProgramAccountsAsync(MetaplexHelpers.ProgramId,
                memCmpList: new List<MemCmp>
                {
                    new () { 
                        Offset = 69, 
                        Bytes = Encoders.Base58.EncodeData(Encoding.UTF8.GetBytes(name)) 
                    },
                    new () { 
                        Offset = 105, 
                        Bytes = Encoders.Base58.EncodeData(Encoding.UTF8.GetBytes(symbol)) 
                    }
                });

            return accounts.Result;
        }
        
        protected async Task<MetadataAccountWrapper> GetArweaveData(MetadataAccountWrapper item) {
    
            if (!item.ArweaveUri.Contains("https://") && !item.ArweaveUri.Contains("http://")) item.ArweaveUri = "https://" + item.ArweaveUri;
            
            _logger.LogInformation($"[{_name}] Requesting arweave data for {item.ArweaveUri} {item.Metadata.Data.Name} - {item.Id}.");
            var strData = await _webClient.DownloadStringTaskAsync(new Uri(item.ArweaveUri));
            //Console.WriteLine(strData);
            if (strData == "")
            {
                _logger.LogInformation($"[{_name}] Something went wrong requesting arweave data for {item.Metadata.Data.Name}.");
                return null;
            }
            try
            {
                _ = JsonDocument.Parse(strData);
            }
            catch (FormatException fex)
            {
                _logger.LogInformation($"[{_name}] Something went wrong requesting arweave data for {item.Mint}. {fex.InnerException?.Message}");
                return null;
            }
            catch (Exception ex) //some other exception
            {
                _logger.LogInformation($"[{_name}] Something went wrong requesting arweave data for {item.Mint}. {ex.InnerException?.Message}");
                return null;
            }
            ArweaveMetadata arweaveData = null;
            try
            {
                arweaveData = JsonSerializer.Deserialize<ArweaveMetadata>(strData, _jsonSerializerOptions);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"[{_name}] Something went wrong requesting arweave data for {item.Mint}. {e.InnerException?.Message}");
                return null;
            }
            if (arweaveData == null)
            {
                return null;
            }
            
            // sanity check because apeshit
            if (!arweaveData.Name.Contains(_sep))
            {
                return null;
            }
            var idSplit = arweaveData.Name.Split(_sep);
            if (idSplit.Length == 0)
            {
                return null;
            }
            var success = int.TryParse(idSplit[1], out var id);
            if (success)
                return new MetadataAccountWrapper()
                {
                    Id = id,
                    Description = arweaveData.Description,
                    Properties = arweaveData.Properties,
                    ExternalUrl = arweaveData.ExternalUrl,
                    Name = arweaveData.Name,
                    Symbol = arweaveData.Symbol,
                    Attributes = arweaveData.Attributes,
                    Image = arweaveData.Image,
                };
            
            // This is because of names that have information after the id
            idSplit = idSplit[1].Split(" ");
            if (idSplit.Length == 0)
                return new MetadataAccountWrapper()
                {
                    Id = id,
                    Description = arweaveData.Description,
                    Properties = arweaveData.Properties,
                    ExternalUrl = arweaveData.ExternalUrl,
                    Name = arweaveData.Name,
                    Symbol = arweaveData.Symbol,
                    Attributes = arweaveData.Attributes,
                    Image = arweaveData.Image,
                };
            
            int.TryParse(idSplit[0], out id);

            return new MetadataAccountWrapper()
            {
                Id = id,
                Description = arweaveData.Description,
                Properties = arweaveData.Properties,
                ExternalUrl = arweaveData.ExternalUrl,
                Name = arweaveData.Name,
                Symbol = arweaveData.Symbol,
                Attributes = arweaveData.Attributes,
                Image = arweaveData.Image,
            };
        }
    }
}