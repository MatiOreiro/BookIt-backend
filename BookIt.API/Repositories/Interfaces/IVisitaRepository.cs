using BookIt.API.Models;

namespace BookIt.API.Repositories.Interfaces;

public interface IVisitaRepository
{
    Task<Visita> CreateAsync(Visita visita);
    Task<Service?> GetServiceByIdAsync(Guid serviceId);
    Task<bool> ExistsPendingOrConfirmedAsync(Guid serviceId, DateTime fechaHoraSolicitada);
    Task<IEnumerable<Visita>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Visita>> GetByServiceIdAsync(Guid serviceId);
}
