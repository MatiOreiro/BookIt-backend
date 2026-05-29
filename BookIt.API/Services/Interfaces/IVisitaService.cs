using BookIt.API.DTOs;

namespace BookIt.API.Services.Interfaces;

public interface IVisitaService
{
    Task<VisitaDto> CreateAsync(Guid currentUserId, CreateVisitaDto dto);
    Task<IEnumerable<VisitaDto>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<VisitaDto>> GetByServiceIdAsync(Guid currentUserId, bool isAdmin, Guid serviceId);
}
