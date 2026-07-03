using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class CreateServiceDto
{
    [Required(ErrorMessage = "El nombre del servicio es obligatorio.")]
    [MaxLength(150)]
    [MinLength(3, ErrorMessage = "El nombre del servicio debe tener al menos 3 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del servicio es obligatoria.")]
    [MaxLength(1000)]
    [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicación es obligatoria.")]
    [MaxLength(255)]
    [MinLength(3, ErrorMessage = "La ubicación debe tener al menos 3 caracteres.")]
    public string Ubicacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de servicio es obligatorio.")]
    [MaxLength(50)]
    [MinLength(3, ErrorMessage = "El tipo de servicio debe tener al menos 3 caracteres.")]
    public string TipoServicio { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio mínimo es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio mínimo debe estar entre 0.01 y 999999.99")]
    public decimal PrecioMinimo { get; set; }

    [Required(ErrorMessage = "El precio máximo es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio máximo debe estar entre 0.01 y 999999.99")]
    public decimal PrecioMaximo { get; set; }

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    public DireccionInputDto Direccion { get; set; } = new();

    [Range(1, 10000, ErrorMessage = "La capacidad debe estar entre 1 y 10000 personas.")]
    public int? Capacidad { get; set; }

    public List<Guid>? CategoryIds { get; set; }

    public List<string>? Images { get; set; }

    // Horarios y días de atención (opcionales; null = usar defaults)
    public List<int>? DiasAtencion { get; set; }

    [Range(0, 23, ErrorMessage = "La hora de apertura debe estar entre 0 y 23.")]
    public int? HoraAperturaReserva { get; set; }

    [Range(1, 24, ErrorMessage = "La hora de cierre debe estar entre 1 y 24.")]
    public int? HoraCierreReserva { get; set; }

    [Range(0, 23, ErrorMessage = "La hora de apertura debe estar entre 0 y 23.")]
    public int? HoraAperturaVisita { get; set; }

    [Range(1, 24, ErrorMessage = "La hora de cierre debe estar entre 1 y 24.")]
    public int? HoraCierreVisita { get; set; }
}