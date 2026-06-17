// BookIt-backend/BookIt.API/DTOs/ConfirmarReservaDto.cs
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class ConfirmarReservaDto
{
    [Required]
    [Range(0.5, 24, ErrorMessage = "Las horas reservadas deben estar entre 0.5 y 24.")]
    public decimal HorasReservadas { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto acordado debe ser mayor a cero.")]
    public decimal MontoAcordado { get; set; }
}
