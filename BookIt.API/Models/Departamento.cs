namespace BookIt.API.Models;

public class Departamento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Barrio> Barrios { get; set; } = new List<Barrio>();
}