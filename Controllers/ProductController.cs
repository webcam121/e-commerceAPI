using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerceAPI.Data;
using ecommerceAPI.Models;
using ecommerceAPI.DTOs;
using System.ComponentModel.DataAnnotations;

namespace ecommerceAPI.Controllers;

/// <summary>
/// Controller for managing products in the e-commerce system
/// </summary>
[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all products with their categories and attributes
    /// </summary>
    /// <returns>A list of products</returns>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.AttributeValues)
                .ThenInclude(pav => pav.AttributeValue)
                    .ThenInclude(av => av.CategoryAttribute)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                ImageBase64 = p.ImageBase64,
                ImageContentType = p.ImageContentType,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsRecommended = p.IsRecommended,
                Attributes = p.AttributeValues.Select(pav => new ProductAttributeDto
                {
                    AttributeId = pav.AttributeValue.CategoryAttributeId,
                    AttributeName = pav.AttributeValue.CategoryAttribute.Name,
                    Value = pav.AttributeValue.Value
                }).ToList()
            })
            .ToListAsync();

        return products;
    }

    /// <summary>
    /// Gets a specific product by ID
    /// </summary>
    /// <param name="id">The ID of the product</param>
    /// <returns>The product with its category and attributes</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.AttributeValues)
                .ThenInclude(pav => pav.AttributeValue)
                    .ThenInclude(av => av.CategoryAttribute)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            ImageBase64 = product.ImageBase64,
            ImageContentType = product.ImageContentType,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            IsRecommended = product.IsRecommended,
            Attributes = product.AttributeValues.Select(pav => new ProductAttributeDto
            {
                AttributeId = pav.AttributeValue.CategoryAttributeId,
                AttributeName = pav.AttributeValue.CategoryAttribute.Name,
                Value = pav.AttributeValue.Value
            }).ToList()
        };
    }

    /// <summary>
    /// Gets all products in a specific category
    /// </summary>
    /// <param name="categoryId">The ID of the category</param>
    /// <returns>A list of products in the category</returns>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProductsByCategory(int categoryId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.AttributeValues)
                .ThenInclude(pav => pav.AttributeValue)
                    .ThenInclude(av => av.CategoryAttribute)
            .Where(p => p.CategoryId == categoryId)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                ImageBase64 = p.ImageBase64,
                ImageContentType = p.ImageContentType,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsRecommended = p.IsRecommended,
                Attributes = p.AttributeValues.Select(pav => new ProductAttributeDto
                {
                    AttributeId = pav.AttributeValue.CategoryAttributeId,
                    AttributeName = pav.AttributeValue.CategoryAttribute.Name,
                    Value = pav.AttributeValue.Value
                }).ToList()
            })
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="name">Product name</param>
    /// <param name="description">Product description</param>
    /// <param name="price">Product price</param>
    /// <param name="stockQuantity">Product stock quantity</param>
    /// <param name="categoryId">Category ID</param>
    /// <param name="image">Product image file</param>
    /// <param name="isRecommended">Whether the product is recommended</param>
    /// <param name="attributeValueIds">List of attribute value IDs</param>
    /// <returns>The created product</returns>
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(
        [FromForm][Required] string name,
        [FromForm] string? description,
        [FromForm][Required] decimal price,
        [FromForm] int stockQuantity,
        [FromForm][Required] int categoryId,
        [FromForm] IFormFile? image,
        [FromForm] bool isRecommended = false,
        [FromForm] List<int>? attributeValueIds = null)
    {
        // Validate category exists
        if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
        {
            return BadRequest("Invalid category ID");
        }

        string? imageBase64 = null;
        string? imageContentType = null;

        // Process image if provided
        if (image != null)
        {
            if (image.Length > 10 * 1024 * 1024) // 10MB limit
            {
                return BadRequest("Image size must be less than 10MB");
            }

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            imageBase64 = Convert.ToBase64String(ms.ToArray());
            imageContentType = image.ContentType;
        }

        // Create new product
        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            CategoryId = categoryId,
            ImageBase64 = imageBase64,
            ImageContentType = imageContentType,
            IsRecommended = isRecommended,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Add attribute values if provided
        if (attributeValueIds != null && attributeValueIds.Any())
        {
            var productAttributeValues = attributeValueIds.Select(attributeValueId => 
                new ProductAttributeValue 
                { 
                    ProductId = product.Id, 
                    AttributeValueId = attributeValueId 
                });

            await _context.ProductAttributeValues.AddRangeAsync(productAttributeValues);
            await _context.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The ID of the product to update</param>
    /// <param name="name">Product name</param>
    /// <param name="description">Product description</param>
    /// <param name="price">Product price</param>
    /// <param name="stockQuantity">Product stock quantity</param>
    /// <param name="categoryId">Category ID</param>
    /// <param name="image">Product image file</param>
    /// <param name="isRecommended">Whether the product is recommended</param>
    /// <param name="attributeValueIds">List of attribute value IDs</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromForm][Required] string name,
        [FromForm] string? description,
        [FromForm][Required] decimal price,
        [FromForm] int stockQuantity,
        [FromForm][Required] int categoryId,
        [FromForm] IFormFile? image,
        [FromForm] bool isRecommended = false,
        [FromForm] List<int>? attributeValueIds = null)
    {
        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        // Validate category exists
        if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
        {
            return BadRequest("Invalid category ID");
        }

        // Process image if provided
        if (image != null)
        {
            if (image.Length > 10 * 1024 * 1024) // 10MB limit
            {
                return BadRequest("Image size must be less than 10MB");
            }

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            existingProduct.ImageBase64 = Convert.ToBase64String(ms.ToArray());
            existingProduct.ImageContentType = image.ContentType;
        }

        // Update properties
        existingProduct.Name = name;
        existingProduct.Description = description;
        existingProduct.Price = price;
        existingProduct.StockQuantity = stockQuantity;
        existingProduct.CategoryId = categoryId;
        existingProduct.IsRecommended = isRecommended;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        // Update attribute values if provided
        if (attributeValueIds != null)
        {
            // Remove existing attribute values
            var existingAttributeValues = await _context.ProductAttributeValues
                .Where(pav => pav.ProductId == id)
                .ToListAsync();
            _context.ProductAttributeValues.RemoveRange(existingAttributeValues);

            // Add new attribute values
            if (attributeValueIds.Any())
            {
                var newAttributeValues = attributeValueIds.Select(attributeValueId =>
                    new ProductAttributeValue
                    {
                        ProductId = id,
                        AttributeValueId = attributeValueId
                    });
                await _context.ProductAttributeValues.AddRangeAsync(newAttributeValues);
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">The ID of the product to delete</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Adds an attribute value to a product
    /// </summary>
    /// <param name="id">The ID of the product</param>
    /// <param name="attributeValue">The attribute value to add</param>
    /// <returns>The updated product</returns>
    [HttpPost("{id}/attributes")]
    public async Task<ActionResult<ProductResponseDto>> AddProductAttribute(int id, ProductAttributeValue attributeValue)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Validate attribute value exists
        if (!await _context.AttributeValues.AnyAsync(av => av.Id == attributeValue.AttributeValueId))
        {
            return BadRequest("Invalid attribute value ID");
        }

        attributeValue.ProductId = id;
        _context.ProductAttributeValues.Add(attributeValue);
        await _context.SaveChangesAsync();

        return await GetProduct(id);
    }

    /// <summary>
    /// Removes an attribute value from a product
    /// </summary>
    /// <param name="id">The ID of the product</param>
    /// <param name="attributeValueId">The ID of the attribute value to remove</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}/attributes/{attributeValueId}")]
    public async Task<IActionResult> RemoveProductAttribute(int id, int attributeValueId)
    {
        var productAttribute = await _context.ProductAttributeValues
            .FirstOrDefaultAsync(pav => pav.ProductId == id && pav.AttributeValueId == attributeValueId);

        if (productAttribute == null)
        {
            return NotFound();
        }

        _context.ProductAttributeValues.Remove(productAttribute);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Filters products by category, category attributes, and attribute values
    /// </summary>
    /// <param name="filter">The filter criteria</param>
    /// <returns>A list of filtered products</returns>
    [HttpPost("filter")]
    public async Task<ActionResult<IEnumerable<Product>>> FilterProducts(ProductFilterDto filter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.AttributeValues)
                .ThenInclude(pav => pav.AttributeValue)
                    .ThenInclude(av => av.CategoryAttribute)
            .AsQueryable();

        // Filter by category
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        // Filter by category attributes and their values
        if (filter.AttributeFilters != null && filter.AttributeFilters.Any())
        {
            foreach (var attrFilter in filter.AttributeFilters)
            {
                query = query.Where(p => p.AttributeValues
                    .Any(pav => pav.AttributeValue.CategoryAttributeId == attrFilter.CategoryAttributeId &&
                               pav.AttributeValue.Value == attrFilter.Value));
            }
        }

        return await query.ToListAsync();
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}

/// <summary>
/// Data transfer object for product filtering
/// </summary>
public class ProductFilterDto
{
    /// <summary>
    /// The ID of the category to filter by
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// The list of attribute filters to apply
    /// </summary>
    public List<AttributeFilterDto>? AttributeFilters { get; set; }
}

/// <summary>
/// Data transfer object for attribute filtering
/// </summary>
public class AttributeFilterDto
{
    /// <summary>
    /// The ID of the category attribute
    /// </summary>
    [Required]
    public int CategoryAttributeId { get; set; }

    /// <summary>
    /// The value of the attribute to filter by
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for product response
/// </summary>
public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ImageBase64 { get; set; }
    public string? ImageContentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsRecommended { get; set; }
    public List<ProductAttributeDto> Attributes { get; set; } = new List<ProductAttributeDto>();
}

/// <summary>
/// Data transfer object for product attribute
/// </summary>
public class ProductAttributeDto
{
    public int AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
} 