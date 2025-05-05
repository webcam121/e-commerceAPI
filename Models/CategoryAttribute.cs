using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ecommerceAPI.Models;

public class CategoryAttribute
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    // Navigation properties
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<AttributeValue> Values { get; set; } = new List<AttributeValue>();
} 