using BookIt.API.Models;

namespace BookIt.API.Repositories.Interfaces;

public interface IReservaRepository
{
    Task<Reserva> CreateAsync(Reserva reserva);
    Task<Reserva?> GetByIdAsync(Guid id);
    Task<IEnumerable<Reserva>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Reserva>> GetByServiceIdAsync(Guid serviceId);
    Task<bool> ExistsBusySlotAsync(Guid serviceId, DateTime fechaReservaCliente);
}
