using BookIt.API.DTOs;

namespace BookIt.API.Services.Interfaces;

public interface IAssistantService
{
    Task<string> AskAboutServiceAsync(ServiceDto service, string pregunta);
    Task<GeneratedFiltersDto> GenerateFiltersAsync(string descripcion);
}
