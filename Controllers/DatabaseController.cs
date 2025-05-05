using Microsoft.AspNetCore.Mvc;
using ecommerceAPI.Data;

namespace ecommerceAPI.Controllers;

/// <summary>
/// Controller for database management operations
/// </summary>
[ApiController]
[Route("api/database")]
public class DatabaseController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DatabaseController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedDatabase()
    {
        await DbSeeder.SeedData(_context);
        return Ok(new { message = "Database seeded successfully" });
    }

    /// <summary>
    /// Reseeds the database with initial data (clears existing data first)
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("reseed")]
    public async Task<IActionResult> ReseedDatabase()
    {
        await DbSeeder.ReseedData(_context);
        return Ok(new { message = "Database reseeded successfully" });
    }
} 