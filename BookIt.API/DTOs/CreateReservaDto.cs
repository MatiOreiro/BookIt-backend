using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class CreateReservaDto
{
    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public DateTime FechaReservaCliente { get; set; }

    [MaxLength(500)]
    public string? Mensaje { get; set; }
}
