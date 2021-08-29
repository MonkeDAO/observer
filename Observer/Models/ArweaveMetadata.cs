using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Observer.Models
{
    /// <summary>
    /// Represents an attribute of the generative non-fungible token collection.
    /// </summary>
    public class Trait
    {
        /// <summary>
        /// The type of the attribute.
        /// </summary>
        [JsonPropertyName("trait_type")]
        public string Type { get; set; }
        
        /// <summary>
        /// The value of the attribute.
        /// </summary>
        public dynamic Value { get; set; }
    }

    /// <summary>
    /// Represents a creator of the non-fungible token on the Arweave hosted metadata.
    /// </summary>
    public class Creator
    {
        /// <summary>
        /// The address of the creator.
        /// </summary>
        public string Address { get; set; }
        
        /// <summary>
        /// The fee share.
        /// </summary>
        public int Share { get; set; }
        
        /// <summary>
        /// The fee share.
        /// </summary>
        public bool Verified { get; set; }
    }

    /// <summary>
    /// Represents the properties of the non-fungible token on the Arweave hosted metadata.
    /// </summary>
    public class Properties
    {
        /// <summary>
        /// The non-fungible token category.
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// The creators of the non-fungible token collection.
        /// </summary>
        public List<Creator> Creators { get; set; }
    }
    
    /// <summary>
    /// Represents the metadata hosted on Arweave.
    /// </summary>
    public class ArweaveMetadata
    {
        /// <summary>
        /// The name of the non-fungible token.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The symbol of the non-fungible token.
        /// </summary>
        public string Symbol { get; set; }
        
        /// <summary>
        /// The description of the non-fungible token.
        /// </summary>
        public string Description { get; set; }
        
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
        /// Some external url..
        /// </summary>
        [JsonPropertyName("animation_url")]
        public string AnimationUrl { get; set; }
        
        /// <summary>
        /// The non-fungible token collection edition.
        /// </summary>
        public string Edition { get; set; }
        
        /// <summary>
        /// The background color of the non-fungible token.
        /// </summary>
        [JsonPropertyName("background_color")]
        public string BackgroundColor { get; set; }
        
        /// <summary>
        /// The non-fungible token attributes.
        /// </summary>
        public List<Trait> Attributes { get; set; }
        
        /// <summary>
        /// The properties of the collection.
        /// </summary>
        public Properties Properties { get; set; }
        
        /// <summary>
        /// A link to the Arweave hosted image.
        /// </summary>
        public string Image { get; set; }
    }
}