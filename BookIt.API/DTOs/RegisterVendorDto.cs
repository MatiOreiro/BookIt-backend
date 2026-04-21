using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class RegisterVendorDto
{
    // User fields
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^09\d{7}$", ErrorMessage = "El teléfono debe ser un número válido de Uruguay (09xxxxxxx - 9 dígitos).")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [MaxLength(200, ErrorMessage = "El email no puede exceder 200 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [MaxLength(128, ErrorMessage = "La contraseña no puede exceder 128 caracteres.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
        ErrorMessage = "La contraseña debe contener mayúsculas, minúsculas, números y caracteres especiales (@$!%*?&).")]
    public string Password { get; set; } = string.Empty;

    // Service fields
    [Required(ErrorMessage = "El nombre del servicio es obligatorio.")]
    [MaxLength(150)]
    [MinLength(3, ErrorMessage = "El nombre del servicio debe tener al menos 3 caracteres.")]
    public string NombreServicio { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del servicio es obligatoria.")]
    [MaxLength(1000)]
    [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
    public string DescripcionServicio { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicación es obligatoria.")]
    [MaxLength(255)]
    [MinLength(3, ErrorMessage = "La ubicación debe tener al menos 3 caracteres.")]
    public string Ubicacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio mínimo es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio mínimo debe estar entre 0.01 y 999999.99")]
    public decimal PrecioMinimo { get; set; }

    [Required(ErrorMessage = "El precio máximo es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio máximo debe estar entre 0.01 y 999999.99")]
    public decimal PrecioMaximo { get; set; }
}
