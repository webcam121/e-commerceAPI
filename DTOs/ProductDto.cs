using System.ComponentModel.DataAnnotations;

namespace ecommerceAPI.DTOs;

public class CreateProductDto
{
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
    
    [Required]
    public int CategoryId { get; set; }
    
    [StringLength(10485760)] // Maximum size for base64 encoded image (10MB)
    public string? ImageBase64 { get; set; }
    
    [StringLength(50)]
    public string? ImageContentType { get; set; }
    
    public bool IsRecommended { get; set; } = false;
    
    public List<int>? AttributeValueIds { get; set; }
} 