// BookIt-backend/BookIt.API/DTOs/ReservaDto.cs
namespace BookIt.API.DTOs;

public class ReservaDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public Guid UserId { get; set; }
    public bool Confirmada { get; set; }
    public DateTime FechaReservaCliente { get; set; }
    public decimal? MontoAcordado { get; set; }
    public decimal? HorasReservadas { get; set; }
    public string? ServiceNombre { get; set; }
    public decimal PrecioMinimo { get; set; }
    public decimal PrecioMaximo { get; set; }
    public string? VendorNombre { get; set; }
    public string? VendorEmail { get; set; }
    public string? VendorTelefono { get; set; }
    public UserDto? Usuario { get; set; }
    public List<PagoDto> Pagos { get; set; } = [];
}
