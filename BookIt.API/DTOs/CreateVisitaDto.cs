using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class CreateVisitaDto
{
    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public DateTime FechaHoraSolicitada { get; set; }

    [MaxLength(500)]
    public string? Mensaje { get; set; }
}
