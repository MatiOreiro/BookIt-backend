using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class GenerateFiltersRequestDto
{
    [Required]
    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;
}
