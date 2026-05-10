using BookIt.API.Data;
using BookIt.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Controllers;

[ApiController]
[Route("event-categories")]
public class EventCategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EventCategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todas las categorías de evento disponibles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEventCategories()
    {
        var categories = await _context.EventCategories
            .OrderBy(category => category.Nombre)
            .Select(category => new EventCategoryDto
            {
                Id = category.Id,
                Nombre = category.Nombre,
                FechaCreacion = category.FechaCreacion
            })
            .ToListAsync();

        return Ok(categories);
    }
}