using BookIt.API.Data;
using BookIt.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Controllers;

[ApiController]
[Route("tags")]
public class TagsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TagsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los tags disponibles para servicios.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTags()
    {
        var tags = await _context.Tags
            .OrderBy(tag => tag.Nombre)
            .Select(tag => new TagDto
            {
                Id = tag.Id,
                Nombre = tag.Nombre,
                FechaCreacion = tag.FechaCreacion
            })
            .ToListAsync();

        return Ok(tags);
    }
}
