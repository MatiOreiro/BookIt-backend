using BookIt.API.DTOs;

namespace BookIt.API.Services.Interfaces;

public interface IServiceService
{
    Task<IEnumerable<ServiceDto>> GetAllAsync();
    Task<IEnumerable<ServiceDto>> GetActiveAsync();
    Task<ServiceDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServiceDto>> GetByVendorIdAsync(Guid vendorId);
    Task<ServiceDto> CreateAsync(Guid currentUserId, CreateServiceDto dto);
    Task<ServiceDto> UpdateAsync(Guid serviceId, Guid currentUserId, bool isAdmin, CreateServiceDto dto);
    Task DeleteAsync(Guid serviceId, Guid currentUserId, bool isAdmin);
    Task<IEnumerable<ServiceDto>> SearchAsync(string? searchTerm, string? location, decimal? minPrice, decimal? maxPrice);
    Task<IEnumerable<ServiceDto>> FilterByPriceAndTypeAsync(decimal? minPrice, decimal? maxPrice, string? tipoServicio, List<Guid>? categoryIds);
    Task<IEnumerable<ServicioAsociadoDto>> GetServiciosAsociadosAsync(Guid salonId);
    Task<ServicioAsociadoDto> AsociarServicioAsync(Guid salonId, Guid serviceId, Guid currentUserId);
    Task QuitarServicioAsociadoAsync(Guid salonId, Guid serviceId, Guid currentUserId);
}
