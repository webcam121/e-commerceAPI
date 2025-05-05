using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerceAPI.Data;
using ecommerceAPI.Models;
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
    public async Task<ActionResult<Product>> GetProduct(int id)
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

        return product;
    }

    /// <summary>
    /// Gets all products in a specific category
    /// </summary>
    /// <param name="categoryId">The ID of the category</param>
    /// <returns>A list of products in the category</returns>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.AttributeValues)
                .ThenInclude(pav => pav.AttributeValue)
                    .ThenInclude(av => av.CategoryAttribute)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="product">The product to create</param>
    /// <returns>The created product</returns>
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        // Validate category exists
        if (!await _context.Categories.AnyAsync(c => c.Id == product.CategoryId))
        {
            return BadRequest("Invalid category ID");
        }

        product.CreatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The ID of the product to update</param>
    /// <param name="product">The updated product data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        // Validate category exists
        if (!await _context.Categories.AnyAsync(c => c.Id == product.CategoryId))
        {
            return BadRequest("Invalid category ID");
        }

        // Update properties
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.StockQuantity = product.StockQuantity;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.ImageBase64 = product.ImageBase64;
        existingProduct.ImageContentType = product.ImageContentType;
        existingProduct.UpdatedAt = DateTime.UtcNow;

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
    public async Task<ActionResult<Product>> AddProductAttribute(int id, ProductAttributeValue attributeValue)
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
    public string AttributeName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
} 