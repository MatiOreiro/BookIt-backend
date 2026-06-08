using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^09\d{7}$", ErrorMessage = "El teléfono debe ser un número válido de Uruguay (09xxxxxxx - 9 dígitos).")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", ErrorMessage = "El email debe incluir un dominio válido (por ejemplo, usuario@dominio.com).")]
    [MaxLength(200, ErrorMessage = "El email no puede exceder 200 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [MaxLength(128, ErrorMessage = "La contraseña no puede exceder 128 caracteres.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
        ErrorMessage = "La contraseña debe contener mayúsculas, minúsculas, números y caracteres especiales (@$!%*?&).")]
    public string Password { get; set; } = string.Empty;

    [MaxLength(50)]
    [RegularExpression(@"^[a-z_]+$", ErrorMessage = "El rol solo puede contener letras minúsculas y guiones bajos.")]
    public string Rol { get; set; } = "usuario";

    public string? ProfileImageUrl { get; set; }
}
