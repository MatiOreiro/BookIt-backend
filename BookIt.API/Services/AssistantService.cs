using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookIt.API.Data;
using BookIt.API.DTOs;
using BookIt.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Services;

public class AssistantService : IAssistantService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AssistantService> _logger;

    private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

    private static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions CaseInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly string[] ValidGuestBuckets =
    {
        "Hasta 50", "50-100", "100-200", "200-300", "Más de 300"
    };

    public AssistantService(
        HttpClient httpClient,
        IConfiguration configuration,
        ApplicationDbContext context,
        ILogger<AssistantService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    public async Task<string> AskAboutServiceAsync(ServiceDto service, string pregunta)
    {
        var systemPrompt = BuildAskSystemPrompt(service);
        var responseText = await CallGroqAsync(systemPrompt, pregunta, jsonMode: false);
        return responseText.Trim();
    }

    public async Task<GeneratedFiltersDto> GenerateFiltersAsync(string descripcion)
    {
        var categories = await _context.EventCategories
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoryItem(c.Id, c.Nombre))
            .ToListAsync();

        var departamentos = await _context.Departamentos
            .OrderBy(d => d.Nombre)
            .Select(d => new DepartamentoItem(d.Id, d.Nombre))
            .ToListAsync();

        var barrios = await _context.Barrios
            .OrderBy(b => b.Nombre)
            .Select(b => new BarrioItem(b.Id, b.Nombre, b.DepartamentoId))
            .ToListAsync();

        var systemPrompt = BuildFiltersSystemPrompt(categories, departamentos, barrios);

        string responseText;
        try
        {
            responseText = await CallGroqAsync(systemPrompt, descripcion, jsonMode: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló la llamada al asistente de IA al generar filtros.");
            return new GeneratedFiltersDto();
        }

        RawFilterResult? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<RawFilterResult>(responseText, CaseInsensitiveOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "No se pudo parsear el JSON de filtros devuelto por el asistente de IA: {Response}", responseText);
            return new GeneratedFiltersDto();
        }

        if (parsed == null)
            return new GeneratedFiltersDto();

        return ValidateAndNormalize(parsed, categories.Select(c => c.Id).ToHashSet(),
            departamentos.Select(d => d.Id).ToHashSet(),
            barrios.ToDictionary(b => b.Id, b => b.DepartamentoId));
    }

    private record CategoryItem(Guid Id, string Nombre);
    private record DepartamentoItem(Guid Id, string Nombre);
    private record BarrioItem(Guid Id, string Nombre, Guid DepartamentoId);

    private static GeneratedFiltersDto ValidateAndNormalize(
        RawFilterResult parsed,
        HashSet<Guid> validCategoryIds,
        HashSet<Guid> validDepartamentoIds,
        Dictionary<Guid, Guid> barrioToDepartamento)
    {
        var result = new GeneratedFiltersDto();

        if (parsed.CategoryIds != null)
        {
            var validIds = parsed.CategoryIds
                .Select(id => Guid.TryParse(id, out var guid) ? guid : (Guid?)null)
                .Where(guid => guid.HasValue && validCategoryIds.Contains(guid.Value))
                .Select(guid => guid!.Value)
                .Distinct()
                .ToList();

            result.CategoryIds = validIds.Count > 0 ? validIds : null;
        }

        if (!string.IsNullOrWhiteSpace(parsed.TipoServicio))
        {
            var tipo = parsed.TipoServicio.Trim();
            if (string.Equals(tipo, "Salón", StringComparison.OrdinalIgnoreCase)
                || string.Equals(tipo, "Salon", StringComparison.OrdinalIgnoreCase))
            {
                result.TipoServicio = "Salón";
            }
            else if (string.Equals(tipo, "Servicio", StringComparison.OrdinalIgnoreCase))
            {
                result.TipoServicio = "Servicio";
            }
        }

        result.MinPrice = parsed.MinPrice;
        result.MaxPrice = parsed.MaxPrice;
        if (result.MinPrice.HasValue && result.MaxPrice.HasValue && result.MinPrice > result.MaxPrice)
        {
            result.MinPrice = null;
            result.MaxPrice = null;
        }

        if (!string.IsNullOrWhiteSpace(parsed.DepartamentoId)
            && Guid.TryParse(parsed.DepartamentoId, out var departamentoId)
            && validDepartamentoIds.Contains(departamentoId))
        {
            result.DepartamentoId = departamentoId;
        }

        if (!string.IsNullOrWhiteSpace(parsed.BarrioId)
            && Guid.TryParse(parsed.BarrioId, out var barrioId)
            && barrioToDepartamento.TryGetValue(barrioId, out var barrioDepartamentoId))
        {
            if (!result.DepartamentoId.HasValue || result.DepartamentoId.Value == barrioDepartamentoId)
            {
                result.BarrioId = barrioId;
                result.DepartamentoId ??= barrioDepartamentoId;
            }
        }

        if (!string.IsNullOrWhiteSpace(parsed.Guests)
            && ValidGuestBuckets.Contains(parsed.Guests.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            result.Guests = ValidGuestBuckets.First(g => string.Equals(g, parsed.Guests.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return result;
    }

    private static string BuildAskSystemPrompt(ServiceDto service)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sos el asistente virtual de BookIt, una plataforma uruguaya de alquiler de salones y servicios para eventos.");
        sb.AppendLine("Respondé siempre en español rioplatense, con tuteo/voseo (por ejemplo: 'tenés', 'podés', 'consultá'), de forma breve (2 a 4 oraciones).");
        sb.AppendLine("Respondé ÚNICAMENTE en base a la información de la publicación que te paso a continuación. Nunca inventes precios, disponibilidad, servicios ni datos que no estén incluidos.");
        sb.AppendLine("Si la pregunta no se puede responder con esta información, decilo explícitamente, por ejemplo: 'No tengo esa información en la publicación, te recomiendo consultarlo directamente con el proveedor.'");
        sb.AppendLine();
        sb.AppendLine("Información de la publicación:");
        sb.AppendLine($"- Nombre: {service.Nombre}");
        sb.AppendLine($"- Tipo: {service.TipoServicio}");
        sb.AppendLine($"- Descripción: {service.Descripcion}");
        sb.AppendLine($"- Ubicación: {service.Ubicacion}");
        if (service.Direccion != null)
        {
            sb.AppendLine($"- Departamento: {service.Direccion.Departamento?.Nombre}");
            sb.AppendLine($"- Barrio: {service.Direccion.Barrio?.Nombre}");
        }
        sb.AppendLine($"- Precio: desde ${service.PrecioMinimo} hasta ${service.PrecioMaximo}");
        if (service.Capacidad.HasValue)
            sb.AppendLine($"- Capacidad: {service.Capacidad} personas");
        if (service.Categorias.Count > 0)
            sb.AppendLine($"- Categorías de evento: {string.Join(", ", service.Categorias.Select(c => c.Nombre))}");
        if (service.DiasAtencion is { Count: > 0 })
            sb.AppendLine($"- Días de atención (0=domingo..6=sábado): {string.Join(", ", service.DiasAtencion)}");
        if (service.HoraAperturaReserva.HasValue || service.HoraCierreReserva.HasValue)
            sb.AppendLine($"- Horario de reservas: {service.HoraAperturaReserva ?? 8}:00 a {service.HoraCierreReserva ?? 22}:00");
        if (service.HoraAperturaVisita.HasValue || service.HoraCierreVisita.HasValue)
            sb.AppendLine($"- Horario de visitas: {service.HoraAperturaVisita ?? 8}:00 a {service.HoraCierreVisita ?? 22}:00");
        if (service.AvgRating.HasValue)
            sb.AppendLine($"- Calificación promedio: {service.AvgRating:F1} ({service.ReviewCount} reseñas)");
        if (service.Vendor != null)
        {
            sb.AppendLine($"- Proveedor: {service.Vendor.Nombre}");
            if (!string.IsNullOrWhiteSpace(service.Vendor.Telefono))
                sb.AppendLine($"- Teléfono de contacto del proveedor: {service.Vendor.Telefono}");
            if (!string.IsNullOrWhiteSpace(service.Vendor.Email))
                sb.AppendLine($"- Email de contacto del proveedor: {service.Vendor.Email}");
        }

        return sb.ToString();
    }

    private static string BuildFiltersSystemPrompt(
        IEnumerable<CategoryItem> categories,
        IEnumerable<DepartamentoItem> departamentos,
        IEnumerable<BarrioItem> barrios)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sos el asistente de búsqueda de BookIt, una plataforma uruguaya de alquiler de salones y servicios para eventos.");
        sb.AppendLine("El usuario va a describir su evento en texto libre. Tu tarea es devolver ÚNICAMENTE un JSON (sin texto adicional, sin markdown) con este esquema exacto:");
        sb.AppendLine("{ \"categoryIds\": [\"guid\", ...] | null, \"tipoServicio\": \"Salón\" | \"Servicio\" | null, \"minPrice\": number | null, \"maxPrice\": number | null, \"departamentoId\": \"guid\" | null, \"barrioId\": \"guid\" | null, \"guests\": \"Hasta 50\" | \"50-100\" | \"100-200\" | \"200-300\" | \"Más de 300\" | null }");
        sb.AppendLine("Reglas: usá EXCLUSIVAMENTE los IDs de las listas de abajo (nunca inventes un GUID). Si no podés inferir un campo con confianza, dejalo en null. Si el usuario menciona una cantidad de invitados, elegí el bucket más cercano.");
        sb.AppendLine();
        sb.AppendLine("Categorías de evento disponibles (id: nombre):");
        foreach (var c in categories)
            sb.AppendLine($"- {c.Id}: {c.Nombre}");
        sb.AppendLine();
        sb.AppendLine("Departamentos disponibles (id: nombre):");
        foreach (var d in departamentos)
            sb.AppendLine($"- {d.Id}: {d.Nombre}");
        sb.AppendLine();
        sb.AppendLine("Barrios disponibles (id: nombre, departamentoId):");
        foreach (var b in barrios)
            sb.AppendLine($"- {b.Id}: {b.Nombre} ({b.DepartamentoId})");

        return sb.ToString();
    }

    private async Task<string> CallGroqAsync(string systemPrompt, string userMessage, bool jsonMode)
    {
        var apiKey = _configuration["GroqSettings:ApiKey"];
        var model = _configuration["GroqSettings:Model"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("GroqSettings:ApiKey no está configurada. Definí 'GroqSettings__ApiKey' en variables de entorno o .env para usar el asistente de IA.");
            throw new InvalidOperationException("El asistente de IA no está configurado.");
        }

        if (string.IsNullOrWhiteSpace(model))
            model = "llama-3.3-70b-versatile";

        var requestBody = new GroqRequest
        {
            Model = model,
            Messages = new List<GroqMessage>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = userMessage }
            },
            Temperature = jsonMode ? 0.2 : 0.4,
            MaxTokens = jsonMode ? 500 : 300,
            ResponseFormat = jsonMode ? new GroqResponseFormat { Type = "json_object" } : null
        };

        var json = JsonSerializer.Serialize(requestBody, SerializeOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, GroqUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Groq devolvió un error {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"Groq respondió con estado {response.StatusCode}.");
        }

        var groqResponse = JsonSerializer.Deserialize<GroqResponse>(responseBody, CaseInsensitiveOptions);
        var text = groqResponse?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogError("Groq no devolvió texto en la respuesta: {Body}", responseBody);
            throw new InvalidOperationException("El asistente de IA no devolvió una respuesta.");
        }

        return text;
    }

    private class GroqRequest
    {
        public string Model { get; set; } = string.Empty;
        public List<GroqMessage> Messages { get; set; } = new();
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
        public GroqResponseFormat? ResponseFormat { get; set; }
    }

    private class GroqMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private class GroqResponseFormat
    {
        public string Type { get; set; } = string.Empty;
    }

    private class GroqResponse
    {
        public List<GroqChoice>? Choices { get; set; }
    }

    private class GroqChoice
    {
        public GroqMessage? Message { get; set; }
    }

    private class RawFilterResult
    {
        public List<string>? CategoryIds { get; set; }
        public string? TipoServicio { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? DepartamentoId { get; set; }
        public string? BarrioId { get; set; }
        public string? Guests { get; set; }
    }
}
