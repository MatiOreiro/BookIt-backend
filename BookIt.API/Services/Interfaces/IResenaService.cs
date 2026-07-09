using BookIt.API.DTOs;

namespace BookIt.API.Services.Interfaces;

public interface IResenaService
{
    Task<ResenaDto> CreateAsync(Guid currentUserId, CreateResenaDto dto);
    Task<IEnumerable<ResenaDto>> GetByServiceIdAsync(Guid serviceId);
}
