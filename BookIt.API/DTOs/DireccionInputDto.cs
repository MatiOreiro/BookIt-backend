using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class DireccionInputDto
{
    [Required(ErrorMessage = "El departamento es obligatorio.")]
    public Guid? DepartamentoId { get; set; }

    [Required(ErrorMessage = "El barrio es obligatorio.")]
    public Guid? BarrioId { get; set; }

    [Required(ErrorMessage = "La calle es obligatoria.")]
    [MaxLength(200, ErrorMessage = "La calle no puede exceder 200 caracteres.")]
    [MinLength(3, ErrorMessage = "La calle debe tener al menos 3 caracteres.")]
    public string Calle { get; set; } = string.Empty;
}