using BookIt.API.DTOs;

namespace BookIt.API.Services.Interfaces;

public interface IPropuestaService
{
    Task<PropuestaDto> CreateAsync(Guid currentUserId, CreatePropuestaDto dto);
    Task<IEnumerable<PropuestaDto>> GetByUserIdAsync(Guid userId);
    Task DeleteAsync(Guid currentUserId, Guid propuestaId);
}
