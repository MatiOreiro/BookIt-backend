// BookIt-backend/BookIt.API/DTOs/PagoDto.cs
namespace BookIt.API.DTOs;

public class PagoDto
{
    public Guid Id { get; set; }
    public Guid ReservaId { get; set; }
    public string TipoPago { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public DateTime FechaPago { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
