using BookIt.API.Models;

namespace BookIt.API.Repositories.Interfaces;

public interface IResenaRepository
{
    Task<Resena> CreateAsync(Resena resena);
    Task<IEnumerable<Resena>> GetByServiceIdAsync(Guid serviceId);
}
