using BookIt.API.DTOs;
using BookIt.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookIt.API.Controllers;

[ApiController]
[Route("resenas")]
public class ResenasController : ControllerBase
{
    private readonly IResenaService _resenaService;

    public ResenasController(IResenaService resenaService)
    {
        _resenaService = resenaService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ResenaDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateResenaDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var resena = await _resenaService.CreateAsync(currentUserId.Value, dto);
        return StatusCode(StatusCodes.Status201Created, resena);
    }

    [HttpGet("service/{serviceId}")]
    [ProducesResponseType(typeof(IEnumerable<ResenaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByService(Guid serviceId)
    {
        var resenas = await _resenaService.GetByServiceIdAsync(serviceId);
        return Ok(resenas);
    }

    private Guid? GetCurrentUserId()
    {
        var rawUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }
}
