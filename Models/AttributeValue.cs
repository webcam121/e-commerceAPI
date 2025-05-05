using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ecommerceAPI.Models;

public class AttributeValue
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Value { get; set; } = string.Empty;
    
    public int CategoryAttributeId { get; set; }

    [JsonIgnore]
    public CategoryAttribute CategoryAttribute { get; set; } = null!;
    
    // Navigation property for product attribute values
    [JsonIgnore]
    public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();
} 