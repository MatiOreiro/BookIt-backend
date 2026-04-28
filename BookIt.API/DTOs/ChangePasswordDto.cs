using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
    [MaxLength(100, ErrorMessage = "La nueva contraseña no puede exceder 100 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debés repetir la nueva contraseña.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas nuevas no coinciden.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}