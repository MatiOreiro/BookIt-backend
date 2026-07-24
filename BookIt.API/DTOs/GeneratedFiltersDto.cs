namespace BookIt.API.DTOs;

public class GeneratedFiltersDto
{
    public List<Guid>? CategoryIds { get; set; }
    public string? TipoServicio { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? DepartamentoId { get; set; }
    public Guid? BarrioId { get; set; }
    public string? Guests { get; set; }
}
