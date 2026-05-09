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
    /// Obtiene todos los tags disponibles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTags()
    {
        var tags = await _context.Tags
            .OrderBy(t => t.Nombre)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Nombre = t.Nombre,
                FechaCreacion = t.FechaCreacion
            })
            .ToListAsync();

        return Ok(tags);
    }

    /// <summary>
    /// Obtiene un tag por su ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTagById(Guid id)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag == null)
            return NotFound(new { message = "Tag no encontrado" });

        return Ok(new TagDto
        {
            Id = tag.Id,
            Nombre = tag.Nombre,
            FechaCreacion = tag.FechaCreacion
        });
    }
}
