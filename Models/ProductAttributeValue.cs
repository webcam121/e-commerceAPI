using System.Text.Json.Serialization;

namespace ecommerceAPI.Models;

public class ProductAttributeValue
{
    public int ProductId { get; set; }
    
    [JsonIgnore]
    public Product Product { get; set; } = null!;
    
    public int AttributeValueId { get; set; }
    public AttributeValue AttributeValue { get; set; } = null!;
} 