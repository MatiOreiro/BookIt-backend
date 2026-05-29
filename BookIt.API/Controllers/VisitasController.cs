using BookIt.API.DTOs;
using BookIt.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookIt.API.Controllers;

[ApiController]
[Route("visitas")]
public class VisitasController : ControllerBase
{
    private readonly IVisitaService _visitaService;

    public VisitasController(IVisitaService visitaService)
    {
        _visitaService = visitaService;
    }

    /// <summary>
    /// Crea una solicitud de visita.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(VisitaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateVisitaDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var visita = await _visitaService.CreateAsync(currentUserId.Value, dto);
        return StatusCode(StatusCodes.Status201Created, visita);
    }

    /// <summary>
    /// Obtiene las visitas del usuario autenticado.
    /// </summary>
    [HttpGet("mis-visitas")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<VisitaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyVisitas()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var visitas = await _visitaService.GetByUserIdAsync(currentUserId.Value);
        return Ok(visitas);
    }

    /// <summary>
    /// Obtiene las visitas de un servicio.
    /// </summary>
    [HttpGet("service/{serviceId}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<VisitaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByService(Guid serviceId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole("administrador");
        var visitas = await _visitaService.GetByServiceIdAsync(currentUserId.Value, isAdmin, serviceId);
        return Ok(visitas);
    }

    private Guid? GetCurrentUserId()
    {
        var rawUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }
}
