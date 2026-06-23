using BookIt.API.DTOs;
using BookIt.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookIt.API.Controllers;

[ApiController]
[Route("reservas")]
public class ReservasController : ControllerBase
{
    private const string AdminRole = "administrador";
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateReservaDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var reserva = await _reservaService.CreateAsync(currentUserId.Value, dto);
        return StatusCode(StatusCodes.Status201Created, reserva);
    }

    [HttpGet("mis-reservas")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ReservaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyReservas()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var reservas = await _reservaService.GetByUserIdAsync(currentUserId.Value);
        return Ok(reservas);
    }

    [HttpGet("service/{serviceId}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ReservaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByService(Guid serviceId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole(AdminRole);
        var reservas = await _reservaService.GetByServiceIdAsync(currentUserId.Value, isAdmin, serviceId);
        return Ok(reservas);
    }

    [HttpPost("desde-visita/{visitaId}")]
    [Authorize]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateFromVisita(Guid visitaId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole(AdminRole);
        var reserva = await _reservaService.CreateFromVisitaAsync(currentUserId.Value, isAdmin, visitaId, new ConfirmarVisitaDto { CrearReserva = true });
        return StatusCode(StatusCodes.Status201Created, reserva);
    }

    [HttpPost("{reservaId}/confirmar")]
    [Authorize]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Confirm(Guid reservaId, [FromBody] ConfirmarReservaDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole(AdminRole);
        var reserva = await _reservaService.ConfirmAsync(currentUserId.Value, isAdmin, reservaId, dto);
        return Ok(reserva);
    }

    [HttpPut("{reservaId}/financiero")]
    [Authorize]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateFinanciero(Guid reservaId, [FromBody] ConfirmarReservaDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole(AdminRole);
        var reserva = await _reservaService.UpdateFinancieroAsync(currentUserId.Value, isAdmin, reservaId, dto);
        return Ok(reserva);
    }

    [HttpDelete("{reservaId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reject(Guid reservaId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole(AdminRole);
        await _reservaService.RejectAsync(currentUserId.Value, isAdmin, reservaId);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var rawUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }
}
