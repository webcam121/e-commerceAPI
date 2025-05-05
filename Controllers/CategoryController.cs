using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerceAPI.Data;
using ecommerceAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ecommerceAPI.Controllers;

/// <summary>
/// Controller for managing categories in the e-commerce system
/// </summary>
[ApiController]
[Route("api/category")]
public class CategoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all categories with their attributes
    /// </summary>
    /// <returns>A list of categories</returns>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
    {
        return await _context.Categories
            .Include(c => c.Attributes)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Attributes = c.Attributes.Select(a => new CategoryAttributeResponseDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description
                }).ToList()
            })
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific category by ID
    /// </summary>
    /// <param name="id">The ID of the category</param>
    /// <returns>The category with its attributes</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponseDto>> GetCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Attributes)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Attributes = c.Attributes.Select(a => new CategoryAttributeResponseDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description
                }).ToList()
            })
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return category;
    }

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="categoryDto">The category data including attribute IDs</param>
    /// <returns>The created category</returns>
    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory(CategoryCreateDto categoryDto)
    {
        var category = new Category
        {
            Name = categoryDto.Name,
            Description = categoryDto.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Add attributes to the category
        if (categoryDto.AttributeIds != null && categoryDto.AttributeIds.Any())
        {
            var attributes = await _context.CategoryAttributes
                .Where(a => categoryDto.AttributeIds.Contains(a.Id))
                .ToListAsync();

            foreach (var attribute in attributes)
            {
                category.Attributes.Add(attribute);
            }

            await _context.SaveChangesAsync();
        }

        // Return the created category with its attributes
        return await GetCategory(category.Id);
    }

    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="id">The ID of the category to update</param>
    /// <param name="categoryDto">The updated category data including attribute IDs</param>
    /// <returns>The updated category</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryResponseDto>> UpdateCategory(int id, CategoryCreateDto categoryDto)
    {
        var existingCategory = await _context.Categories
            .Include(c => c.Attributes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existingCategory == null)
        {
            return NotFound();
        }

        // Update basic properties
        existingCategory.Name = categoryDto.Name;
        existingCategory.Description = categoryDto.Description;
        existingCategory.UpdatedAt = DateTime.UtcNow;

        // Update attributes
        if (categoryDto.AttributeIds != null)
        {
            // Clear existing attributes
            existingCategory.Attributes.Clear();

            // Add new attributes
            if (categoryDto.AttributeIds.Any())
            {
                var attributes = await _context.CategoryAttributes
                    .Where(a => categoryDto.AttributeIds.Contains(a.Id))
                    .ToListAsync();

                foreach (var attribute in attributes)
                {
                    existingCategory.Attributes.Add(attribute);
                }
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoryExists(id))
            {
                return NotFound();
            }
            throw;
        }

        // Return the updated category with its attributes
        return await GetCategory(id);
    }

    /// <summary>
    /// Deletes a category
    /// </summary>
    /// <param name="id">The ID of the category to delete</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }
}

/// <summary>
/// Data transfer object for category response
/// </summary>
public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CategoryAttributeResponseDto> Attributes { get; set; } = new List<CategoryAttributeResponseDto>();
}

/// <summary>
/// Data transfer object for category attribute response
/// </summary>
public class CategoryAttributeResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Data transfer object for creating or updating a category
/// </summary>
public class CategoryCreateDto
{
    /// <summary>
    /// The name of the category
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the category
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// The IDs of the attributes to associate with the category
    /// </summary>
    public List<int>? AttributeIds { get; set; }
} 