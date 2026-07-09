using BookIt.API.DTOs;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using BookIt.API.Services.Interfaces;
using System.Text.Json;

namespace BookIt.API.Services;

public class ResenaService : IResenaService
{
    private const int MaxMediaUrls = 6;

    private readonly IResenaRepository _resenaRepository;
    private readonly IReservaRepository _reservaRepository;

    public ResenaService(IResenaRepository resenaRepository, IReservaRepository reservaRepository)
    {
        _resenaRepository = resenaRepository;
        _reservaRepository = reservaRepository;
    }

    public async Task<ResenaDto> CreateAsync(Guid currentUserId, CreateResenaDto dto)
    {
        var reserva = await _reservaRepository.GetByIdAsync(dto.ReservaId)
            ?? throw new KeyNotFoundException("Reserva no encontrada.");

        if (reserva.UserId != currentUserId)
            throw new UnauthorizedAccessException("No podés reseñar una reserva que no te pertenece.");

        if (!reserva.EsRealizada())
            throw new ArgumentException("La reserva todavía no fue realizada.");

        if (reserva.Resena != null)
            throw new InvalidOperationException("Ya existe una reseña para esta reserva.");

        var mediaUrls = NormalizeMediaUrls(dto.MediaUrls);

        var resena = new Resena
        {
            ReservaId = reserva.Id,
            ServiceId = reserva.ServiceId,
            UserId = currentUserId,
            Puntuacion = dto.Puntuacion,
            Comentario = string.IsNullOrWhiteSpace(dto.Comentario) ? null : dto.Comentario.Trim(),
            MediaUrlsJson = mediaUrls.Count == 0 ? null : JsonSerializer.Serialize(mediaUrls)
        };

        var created = await _resenaRepository.CreateAsync(resena);
        created.User = reserva.User;

        return MapToDto(created);
    }

    public async Task<IEnumerable<ResenaDto>> GetByServiceIdAsync(Guid serviceId)
    {
        var resenas = await _resenaRepository.GetByServiceIdAsync(serviceId);
        return resenas.Select(MapToDto);
    }

    private static List<string> NormalizeMediaUrls(IEnumerable<string>? mediaUrls)
    {
        var normalized = new List<string>();

        foreach (var url in mediaUrls ?? Enumerable.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(url))
                continue;

            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var parsedUri))
                throw new ArgumentException("Cada archivo debe ser una URL absoluta válida.");

            normalized.Add(parsedUri.ToString());
        }

        if (normalized.Count > MaxMediaUrls)
            throw new ArgumentException($"No se pueden adjuntar más de {MaxMediaUrls} archivos.");

        return normalized;
    }

    private static List<string> GetMediaUrls(string? mediaUrlsJson)
    {
        if (string.IsNullOrWhiteSpace(mediaUrlsJson))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(mediaUrlsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static ResenaDto MapToDto(Resena resena) => new()
    {
        Id = resena.Id,
        ReservaId = resena.ReservaId,
        ServiceId = resena.ServiceId,
        UserId = resena.UserId,
        Puntuacion = resena.Puntuacion,
        Comentario = resena.Comentario,
        MediaUrls = GetMediaUrls(resena.MediaUrlsJson),
        FechaCreacion = resena.FechaCreacion,
        FechaActualizacion = resena.FechaActualizacion,
        UsuarioNombre = resena.User?.Nombre,
        UsuarioProfileImageUrl = resena.User?.ProfileImageUrl
    };
}
