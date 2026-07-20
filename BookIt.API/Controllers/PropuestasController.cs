using BookIt.API.DTOs;
using BookIt.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookIt.API.Controllers;

[ApiController]
[Route("propuestas")]
public class PropuestasController : ControllerBase
{
    private readonly IPropuestaService _propuestaService;

    public PropuestasController(IPropuestaService propuestaService)
    {
        _propuestaService = propuestaService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PropuestaDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePropuestaDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var propuesta = await _propuestaService.CreateAsync(currentUserId.Value, dto);
        return StatusCode(StatusCodes.Status201Created, propuesta);
    }

    [HttpGet("mis-propuestas")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<PropuestaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPropuestas()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var propuestas = await _propuestaService.GetByUserIdAsync(currentUserId.Value);
        return Ok(propuestas);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        await _propuestaService.DeleteAsync(currentUserId.Value, id);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var rawUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }
}
