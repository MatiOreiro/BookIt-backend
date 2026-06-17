// BookIt-backend/BookIt.API/DTOs/CreatePagoDto.cs
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class CreatePagoDto
{
    [Required]
    public Guid ReservaId { get; set; }

    [Required]
    [MaxLength(20)]
    public string TipoPago { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor a cero.")]
    public decimal Importe { get; set; }

    [Required]
    public DateTime FechaPago { get; set; }
}
