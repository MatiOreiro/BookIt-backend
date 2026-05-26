namespace BookIt.API.DTOs;

public class ReservaDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public Guid UserId { get; set; }
    public bool Confirmada { get; set; }
    public DateTime FechaReservaCliente { get; set; }
    public UserDto? Usuario { get; set; }
}