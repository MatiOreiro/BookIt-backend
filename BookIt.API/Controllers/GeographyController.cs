using BookIt.API.Data;
using BookIt.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Controllers;

[ApiController]
[Route("departamentos")]
public class GeographyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GeographyController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los departamentos disponibles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DepartamentoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartamentos()
    {
        var departamentos = await _context.Departamentos
            .OrderBy(departamento => departamento.Nombre)
            .Select(departamento => new DepartamentoDto
            {
                Id = departamento.Id,
                Nombre = departamento.Nombre
            })
            .ToListAsync();

        return Ok(departamentos);
    }

    /// <summary>
    /// Obtiene los barrios de un departamento específico.
    /// </summary>
    [HttpGet("{departamentoId:guid}/barrios")]
    [ProducesResponseType(typeof(IEnumerable<BarrioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBarriosByDepartamentoId(Guid departamentoId)
    {
        var barrios = await _context.Barrios
            .Where(barrio => barrio.DepartamentoId == departamentoId)
            .OrderBy(barrio => barrio.Nombre)
            .Select(barrio => new BarrioDto
            {
                Id = barrio.Id,
                DepartamentoId = barrio.DepartamentoId,
                Nombre = barrio.Nombre,
                Departamento = null
            })
            .ToListAsync();

        return Ok(barrios);
    }
}