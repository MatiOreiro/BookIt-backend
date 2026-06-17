// BookIt-backend/BookIt.API/Controllers/PagosController.cs
using BookIt.API.DTOs;
using BookIt.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookIt.API.Controllers;

[ApiController]
[Route("pagos")]
public class PagosController : ControllerBase
{
    private const string AdminRole = "administrador";
    private readonly IPagoService _pagoService;

    public PagosController(IPagoService pagoService)
    {
        _pagoService = pagoService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PagoDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePagoDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var isAdmin = User.IsInRole(AdminRole);
        var pago = await _pagoService.CreateAsync(currentUserId.Value, isAdmin, dto);
        return StatusCode(StatusCodes.Status201Created, pago);
    }

    [HttpPut("{pagoId}")]
    [Authorize]
    [ProducesResponseType(typeof(PagoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid pagoId, [FromBody] UpdatePagoDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var isAdmin = User.IsInRole(AdminRole);
        var pago = await _pagoService.UpdateAsync(currentUserId.Value, isAdmin, pagoId, dto);
        return Ok(pago);
    }

    [HttpGet("reserva/{reservaId}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<PagoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByReserva(Guid reservaId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var isAdmin = User.IsInRole(AdminRole);
        var pagos = await _pagoService.GetByReservaIdAsync(currentUserId.Value, isAdmin, reservaId);
        return Ok(pagos);
    }

    private Guid? GetCurrentUserId()
    {
        var rawUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }
}
