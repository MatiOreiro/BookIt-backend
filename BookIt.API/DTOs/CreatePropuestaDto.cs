using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class CreatePropuestaDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El salonId es obligatorio.")]
    public Guid SalonId { get; set; }

    [Required(ErrorMessage = "Debe incluir al menos un servicio.")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un servicio.")]
    public List<Guid> ServiceIds { get; set; } = new();
}
