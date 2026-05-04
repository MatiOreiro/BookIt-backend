using BookIt.API.DTOs;
using BookIt.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.API.Controllers;

[ApiController]
[Route("services")]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    /// <summary>
    /// Obtiene todos los servicios disponibles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var services = await _serviceService.GetAllAsync();
        return Ok(services);
    }

    /// <summary>
    /// Obtiene solo los servicios activos.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var services = await _serviceService.GetActiveAsync();
        return Ok(services);
    }

    /// <summary>
    /// Obtiene un servicio por su ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
            return NotFound(new { message = "Servicio no encontrado" });

        return Ok(service);
    }

    /// <summary>
    /// Busca servicios por término, ubicación y rango de precios.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] string? location,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice)
    {
        var services = await _serviceService.SearchAsync(searchTerm, location, minPrice, maxPrice);
        return Ok(services);
    }

    /// <summary>
    /// Obtiene todos los servicios de un vendedor.
    /// </summary>
    [HttpGet("vendor/{vendorId}")]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByVendorId(Guid vendorId)
    {
        var services = await _serviceService.GetByVendorIdAsync(vendorId);
        return Ok(services);
    }
}
