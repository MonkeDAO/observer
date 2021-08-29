using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Observer.Models
{
    /// <summary>
    /// A wrapper of the metadata account which includes arweave data.
    /// </summary>
    public class MetadataAccountWrapper
    {
        [JsonIgnore]
        public MetadataAccount Metadata { get; set; }
        
        /// <summary>
        /// The id of the non-fungible token.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The description of the non-fungible token.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// The name of the non-fungible token.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The symbol of the collection.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The address of the mint.
        /// </summary>
        public string Mint { get; set; }

        /// <summary>
        /// The address of the metadata account.
        /// </summary>
        public string Address { get; set; }
        
        /// <summary>
        /// The hash of the endpoint for the arweave hosted image.
        /// </summary>
        public string Image { get; set; }
        
        /// <summary>
        /// The uri to arweave hosted data.
        /// </summary>
        public string ArweaveUri { get; set; }

        /// <summary>
        /// The seller fees in basis points.
        /// </summary>
        [JsonPropertyName("seller_fee_basis_points")]
        public int SellerFeeBasisPoints { get; set; }
        
        /// <summary>
        /// Some external url..
        /// </summary>
        [JsonPropertyName("external_url")]
        public string ExternalUrl { get; set; }

        /// <summary>
        /// The non-fungible token attributes.
        /// </summary>
        public List<Trait> Attributes { get; set; }
        
        /// <summary>
        /// Properties of the collection.
        /// </summary>
        public Properties Properties { get; set; }
    }

    public class AttributesWrapper
    {
        /// <summary>
        /// The non-fungible token attributes.
        /// </summary>
        public List<Trait> Attributes { get; set; }
    }


    public class MonkeyWrapper
    {
        /// <summary>
        /// The id of the non-fungible token.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The address of the mint.
        /// </summary>
        public string Mint { get; set; }
        /// <summary>
        /// The hash of the endpoint for the arweave hosted image.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// The non-fungible token attributes.
        /// </summary>
        public AttributesWrapper Attributes { get; set; }
    }
}