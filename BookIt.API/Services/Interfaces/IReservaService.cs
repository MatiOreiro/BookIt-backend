using BookIt.API.DTOs;

namespace BookIt.API.Services.Interfaces;

public interface IReservaService
{
    Task<ReservaDto> CreateAsync(Guid currentUserId, CreateReservaDto dto);
    Task<IEnumerable<ReservaDto>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<ReservaDto>> GetByServiceIdAsync(Guid currentUserId, bool isAdmin, Guid serviceId);
    Task<ReservaDto> CreateFromVisitaAsync(Guid currentUserId, bool isAdmin, Guid visitaId);
    Task<ReservaDto> ConfirmAsync(Guid currentUserId, bool isAdmin, Guid reservaId, ConfirmarReservaDto dto);
    Task<ReservaDto> UpdateFinancieroAsync(Guid currentUserId, bool isAdmin, Guid reservaId, ConfirmarReservaDto dto);
    Task RejectAsync(Guid currentUserId, bool isAdmin, Guid reservaId);
}
