using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class CreateResenaDto
{
    [Required]
    public Guid ReservaId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5.")]
    public int Puntuacion { get; set; }

    [MaxLength(2000)]
    public string? Comentario { get; set; }

    public List<string>? MediaUrls { get; set; }
}
