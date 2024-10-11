using catApplication.Configuration;
using catApplication.Dtos;
using catApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace catApplication.Controllers;

[ApiController]
[Route("[controller]")]
public class CatController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ICatService _catService;

    public CatController(DatabaseContext context, ICatService catService)
    {
        _context = context;
        _catService = catService;
    }
    
    /// <summary>
    /// Fetch and save 25 random cats from the external Cat API.
    /// </summary>
    /// <returns>Returns a list of cat images</returns>
    [HttpPost("fetch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FetchCats([FromQuery] int count = 25)
    {
        try
        {
            await _catService.FetchAndSaveCatsAsync(count);
            return Ok(new { message = $"{count} cats fetched and saved successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching cats.", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get cat by ID.
    /// </summary>
    /// <param name="id">Cat ID</param>
    /// <returns>Returns a specific cat by ID</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCatById(int id)
    {
        var cat = await _catService.GetCatByIdAsync(id);

        if (cat == null)
        {
            return NotFound(new { message = $"Cat with ID {id} not found." });
        }

        return Ok(cat);
    }
    
    /// <summary>
    /// Get cats with pagination.
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Returns a paginated list of cats</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCats([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var cats = await _catService.GetCatsAsync(page, pageSize);
        return Ok(cats);
    }
    
    /// <summary>
    /// Get cats filtered by a specific tag with pagination.
    /// </summary>
    /// <param name="tag">Cat tag</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Returns a paginated list of cats filtered by the tag</returns>
    [HttpGet("tag")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCatsByTag([FromQuery] string tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var cats = await _catService.GetCatsByTagAsync(tag, page, pageSize);
    
        if (cats == null || !cats.Any())
        {
            return NotFound(new { message = $"No cats found with tag '{tag}'." });
        }

        return Ok(cats);
    }
}