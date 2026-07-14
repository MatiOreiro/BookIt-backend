// BookIt-backend/BookIt.API/Models/Reserva.cs
namespace BookIt.API.Models;

public class Reserva
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceId { get; set; }
    public Guid UserId { get; set; }
    public bool Confirmada { get; set; } = false;
    public DateTime FechaReservaCliente { get; set; }
    public decimal? MontoAcordado { get; set; }
    public decimal? HorasReservadas { get; set; }

    public Service? Service { get; set; }
    public User? User { get; set; }
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public Resena? Resena { get; set; }

    public bool EsRealizada() =>
        Confirmada &&
        (HorasReservadas.HasValue
            ? FechaReservaCliente.AddHours((double)HorasReservadas.Value)
            : FechaReservaCliente.AddMinutes(30)) <= DateTime.UtcNow;
}
