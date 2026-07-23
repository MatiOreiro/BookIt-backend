using System.ComponentModel.DataAnnotations;

namespace BookIt.API.DTOs;

public class AskAssistantDto
{
    [Required]
    [MaxLength(500)]
    public string Pregunta { get; set; } = string.Empty;
}
