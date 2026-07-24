using BookIt.API.DTOs;
using BookIt.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookIt.API.Controllers;

[ApiController]
[Route("services")]
[EnableRateLimiting("ai")]
public class AssistantController : ControllerBase
{
    private readonly IAssistantService _assistantService;
    private readonly IServiceService _serviceService;

    public AssistantController(IAssistantService assistantService, IServiceService serviceService)
    {
        _assistantService = assistantService;
        _serviceService = serviceService;
    }

    /// <summary>
    /// Responde una pregunta del usuario usando como contexto la información del salón/servicio.
    /// </summary>
    [HttpPost("{id}/assistant/ask")]
    [ProducesResponseType(typeof(AssistantResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ask(Guid id, [FromBody] AskAssistantDto dto)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
            return NotFound(new { message = "Servicio no encontrado" });

        var respuesta = await _assistantService.AskAboutServiceAsync(service, dto.Pregunta);
        return Ok(new AssistantResponseDto { Respuesta = respuesta });
    }

    /// <summary>
    /// Genera filtros de búsqueda a partir de una descripción libre del evento.
    /// </summary>
    [HttpPost("filters/generate")]
    [ProducesResponseType(typeof(GeneratedFiltersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateFilters([FromBody] GenerateFiltersRequestDto dto)
    {
        var filters = await _assistantService.GenerateFiltersAsync(dto.Descripcion);
        return Ok(filters);
    }
}
