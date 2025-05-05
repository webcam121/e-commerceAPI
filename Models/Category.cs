using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ecommerceAPI.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<CategoryAttribute> Attributes { get; set; } = new List<CategoryAttribute>();
    
    [JsonIgnore] // This will prevent the Products collection from being serialized
    public ICollection<Product> Products { get; set; } = new List<Product>();
} 