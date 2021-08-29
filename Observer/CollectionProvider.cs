using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Observer.Models;

namespace Observer
{
    /// <summary>
    /// Implements a collection provider for metadata accounts.
    /// </summary>
    public class CollectionProvider
    {
        /// <summary>
        /// The metadata accounts. Yes, this holds thousands of them. Yolo.
        /// </summary>
        private readonly List<MetadataAccountWrapper> _metadataAccounts;
        
        /// <summary>
        /// The json serializer options.
        /// </summary>
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        
        /// <summary>
        /// Logger instance.
        /// </summary>
        protected ILogger _logger;

        public CollectionProvider(ILogger logger)
        {
            _logger = logger;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            _metadataAccounts = new List<MetadataAccountWrapper>();
        }
        
        /// <summary>
        /// Loads metadatas from their respective jsons inside the resources directory
        /// </summary>
        public async Task LoadMetadata()
        {
            // add collection names here
            var collections = new List<string>
            {
                "SolanaDoge",
                "SMB",
                "DegenApe",
            };

            foreach (var coll in collections)
            {
                _logger.LogInformation($"Loading metadata for {coll}.");
                var data = await File.ReadAllTextAsync($"Resources/{coll}.json");
                var metadataAccounts = JsonSerializer.Deserialize<List<MetadataAccountWrapper>>(data,
                    _jsonSerializerOptions);
                if (metadataAccounts != null) 
                    _metadataAccounts.AddRange(metadataAccounts.Where(
                        x => metadataAccounts.FindAll(y => y.Id == x.Id).Count == 1));
            }
        }

        /// <summary>
        /// Get a metadata account for the given mint.
        /// </summary>
        /// <param name="key">The nft mint.</param>
        /// <returns>The metadata account or null.</returns>
        public MetadataAccountWrapper GetMetadataAccountForMint(string key)
        {
            return _metadataAccounts.FirstOrDefault(x => x.Mint == key);
        }
    }
}