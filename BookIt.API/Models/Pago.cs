// BookIt-backend/BookIt.API/Models/Pago.cs
namespace BookIt.API.Models;

public class Pago
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReservaId { get; set; }
    public string TipoPago { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public DateTime FechaPago { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public Reserva? Reserva { get; set; }
}
