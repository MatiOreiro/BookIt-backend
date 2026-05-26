namespace BookIt.API.Models;

public class Reserva
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceId { get; set; }
    public Guid UserId { get; set; }
    public bool Confirmada { get; set; } = false;
    public DateTime FechaReservaCliente { get; set; }

    public Service? Service { get; set; }
    public User? User { get; set; }
}