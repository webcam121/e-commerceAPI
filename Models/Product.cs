using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ecommerceAPI.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    
    public int CategoryId { get; set; }

    [JsonIgnore]
    public Category Category { get; set; } = null!;
    
    // Base64 encoded image
    [StringLength(10485760)] // Maximum size for base64 encoded image (10MB)
    public string? ImageBase64 { get; set; }
    
    // Image metadata
    [StringLength(50)]
    public string? ImageContentType { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Recommended flag
    public bool IsRecommended { get; set; } = false;
    
    // Navigation properties
    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
} 