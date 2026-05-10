using BookIt.API.DTOs;

namespace BookIt.API.Services.Interfaces;

public interface IServiceService
{
    Task<IEnumerable<ServiceDto>> GetAllAsync();
    Task<IEnumerable<ServiceDto>> GetActiveAsync();
    Task<ServiceDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServiceDto>> GetByVendorIdAsync(Guid vendorId);
    Task<IEnumerable<ServiceDto>> SearchAsync(string? searchTerm, string? location, decimal? minPrice, decimal? maxPrice);
    Task<IEnumerable<ServiceDto>> FilterByPriceAndTypeAsync(decimal? minPrice, decimal? maxPrice, string? tipoServicio, List<Guid>? categoryIds);
}
