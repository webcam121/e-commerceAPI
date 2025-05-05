using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerceAPI.Data;
using ecommerceAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ecommerceAPI.Controllers;

/// <summary>
/// Controller for managing attributes in the e-commerce system
/// </summary>
[ApiController]
[Route("api/attribute")]
public class AttributeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AttributeController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all attributes with their values
    /// </summary>
    /// <returns>A list of attributes with their values</returns>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<AttributeResponseDto>>> GetAttributes()
    {
        return await _context.CategoryAttributes
            .Include(a => a.Values)
            .Select(a => new AttributeResponseDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                Values = a.Values.Select(v => v.Value).ToList()
            })
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific attribute with its values
    /// </summary>
    /// <param name="id">The ID of the attribute</param>
    /// <returns>The attribute with its values</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<AttributeResponseDto>> GetAttribute(int id)
    {
        var attribute = await _context.CategoryAttributes
            .Include(a => a.Values)
            .Select(a => new AttributeResponseDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                Values = a.Values.Select(v => v.Value).ToList()
            })
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attribute == null)
        {
            return NotFound();
        }

        return attribute;
    }

    /// <summary>
    /// Creates a new attribute
    /// </summary>
    /// <param name="attributeDto">The attribute data</param>
    /// <returns>The created attribute</returns>
    [HttpPost]
    public async Task<ActionResult<AttributeResponseDto>> CreateAttribute(AttributeCreateDto attributeDto)
    {
        var attribute = new CategoryAttribute
        {
            Name = attributeDto.Name,
            Description = attributeDto.Description
        };

        _context.CategoryAttributes.Add(attribute);
        await _context.SaveChangesAsync();

        // Add attribute values if provided
        if (attributeDto.Values != null && attributeDto.Values.Any())
        {
            var attributeValues = attributeDto.Values
                .Select(v => new AttributeValue
                {
                    Value = v,
                    CategoryAttributeId = attribute.Id
                })
                .ToList();

            await _context.AttributeValues.AddRangeAsync(attributeValues);
            await _context.SaveChangesAsync();
        }

        return await GetAttribute(attribute.Id);
    }

    /// <summary>
    /// Updates an existing attribute
    /// </summary>
    /// <param name="id">The ID of the attribute to update</param>
    /// <param name="attributeDto">The updated attribute data</param>
    /// <returns>The updated attribute</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<AttributeResponseDto>> UpdateAttribute(int id, AttributeCreateDto attributeDto)
    {
        var existingAttribute = await _context.CategoryAttributes
            .Include(a => a.Values)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (existingAttribute == null)
        {
            return NotFound();
        }

        // Update basic properties
        existingAttribute.Name = attributeDto.Name;
        existingAttribute.Description = attributeDto.Description;

        // Update attribute values if provided
        if (attributeDto.Values != null)
        {
            // Remove existing values
            _context.AttributeValues.RemoveRange(existingAttribute.Values);

            // Add new values
            if (attributeDto.Values.Any())
            {
                var attributeValues = attributeDto.Values
                    .Select(v => new AttributeValue
                    {
                        Value = v,
                        CategoryAttributeId = id
                    })
                    .ToList();

                await _context.AttributeValues.AddRangeAsync(attributeValues);
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AttributeExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return await GetAttribute(id);
    }

    /// <summary>
    /// Deletes an attribute
    /// </summary>
    /// <param name="id">The ID of the attribute to delete</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttribute(int id)
    {
        var attribute = await _context.CategoryAttributes
            .Include(a => a.Values)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attribute == null)
        {
            return NotFound();
        }

        // Remove all attribute values first
        _context.AttributeValues.RemoveRange(attribute.Values);
        
        // Then remove the attribute
        _context.CategoryAttributes.Remove(attribute);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool AttributeExists(int id)
    {
        return _context.CategoryAttributes.Any(e => e.Id == id);
    }
}

/// <summary>
/// Data transfer object for attribute response
/// </summary>
public class AttributeResponseDto
{
    /// <summary>
    /// The ID of the attribute
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the attribute
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the attribute
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The list of values for this attribute
    /// </summary>
    public List<string> Values { get; set; } = new List<string>();
}

/// <summary>
/// Data transfer object for creating or updating an attribute
/// </summary>
public class AttributeCreateDto
{
    /// <summary>
    /// The name of the attribute
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the attribute
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// The list of values for this attribute
    /// </summary>
    public List<string>? Values { get; set; }
} 