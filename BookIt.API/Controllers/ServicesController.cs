using BookIt.API.DTOs;
using BookIt.API.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// Crea un servicio para el usuario autenticado.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var service = await _serviceService.CreateAsync(currentUserId.Value, dto);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    /// <summary>
    /// Actualiza un servicio existente del vendedor autenticado.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateServiceDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole("administrador");
        var service = await _serviceService.UpdateAsync(id, currentUserId.Value, isAdmin, dto);
        return Ok(service);
    }

    /// <summary>
    /// Elimina un servicio. Si era el último del vendedor, el usuario vuelve a rol usuario.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole("administrador");
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
            return NotFound(new { message = "Servicio no encontrado" });

        if (!isAdmin && service.VendorId != currentUserId.Value)
            return Forbid();

        await _serviceService.DeleteAsync(id, currentUserId.Value, isAdmin);
        return NoContent();
    }

    /// <summary>
    /// Filtra servicios por precio y tipo de servicio.
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FilterByPriceAndType(
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? tipoServicio,
        [FromQuery] List<Guid>? categoryIds)
    {
        // Validar que minPrice no sea mayor que maxPrice
        if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
            return BadRequest(new { message = "El precio mínimo no puede ser mayor que el precio máximo" });

        var services = await _serviceService.FilterByPriceAndTypeAsync(minPrice, maxPrice, tipoServicio, categoryIds);
        return Ok(services);
    }

    /// <summary>
    /// Obtiene los servicios asociados a un salón.
    /// </summary>
    [HttpGet("{salonId}/servicios-asociados")]
    [ProducesResponseType(typeof(IEnumerable<ServicioAsociadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiciosAsociados(Guid salonId)
    {
        var servicios = await _serviceService.GetServiciosAsociadosAsync(salonId);
        return Ok(servicios);
    }

    /// <summary>
    /// Asocia un servicio a un salón. Solo el dueño del salón puede hacerlo.
    /// </summary>
    [HttpPost("{salonId}/servicios-asociados")]
    [Authorize]
    [ProducesResponseType(typeof(ServicioAsociadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsociarServicio(Guid salonId, [FromBody] CreateSalonServiceDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var servicio = await _serviceService.AsociarServicioAsync(salonId, dto.ServiceId, currentUserId.Value);
        return StatusCode(StatusCodes.Status201Created, servicio);
    }

    /// <summary>
    /// Quita la asociación de un servicio a un salón. Solo el dueño del salón puede hacerlo.
    /// </summary>
    [HttpDelete("{salonId}/servicios-asociados/{serviceId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QuitarServicioAsociado(Guid salonId, Guid serviceId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        await _serviceService.QuitarServicioAsociadoAsync(salonId, serviceId, currentUserId.Value);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var rawUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }
}
