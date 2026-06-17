// BookIt-backend/BookIt.API/Services/Interfaces/IPagoService.cs
using BookIt.API.DTOs;

namespace BookIt.API.Services.Interfaces;

public interface IPagoService
{
    Task<PagoDto> CreateAsync(Guid currentUserId, bool isAdmin, CreatePagoDto dto);
    Task<PagoDto> UpdateAsync(Guid currentUserId, bool isAdmin, Guid pagoId, UpdatePagoDto dto);
    Task<IEnumerable<PagoDto>> GetByReservaIdAsync(Guid currentUserId, bool isAdmin, Guid reservaId);
}
