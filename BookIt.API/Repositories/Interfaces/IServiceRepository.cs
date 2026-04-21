using BookIt.API.Models;

namespace BookIt.API.Repositories.Interfaces;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id);
    Task<Service?> GetByVendorIdAsync(Guid vendorId);
    Task<IEnumerable<Service>> GetAllAsync();
    Task<IEnumerable<Service>> GetActiveAsync();
    Task<Service> CreateAsync(Service service);
    Task<Service> UpdateAsync(Service service);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsByIdAsync(Guid id);
}
