using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class CreateSalonServiceDto
{
    [Required(ErrorMessage = "El serviceId es obligatorio.")]
    public Guid ServiceId { get; set; }
}
