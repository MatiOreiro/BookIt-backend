namespace BookIt.API.DTOs;

public class ConfirmarVisitaDto
{
    public bool CrearReserva { get; set; }
    public DateTime? FechaReservaCliente { get; set; }
    public decimal? HorasReservadas { get; set; }
    public decimal? MontoAcordado { get; set; }
}
