using BookIt.API.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace BookIt.API.Services;

public class FileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif"
    };

    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveSingleAsync(IFormFile? file, string folderName, string filePrefix)
    {
        if (file == null || file.Length == 0)
            return null;

        var savedFiles = await SaveFilesInternalAsync([file], folderName, filePrefix);
        return savedFiles.FirstOrDefault();
    }

    public Task<List<string>> SaveManyAsync(IEnumerable<IFormFile>? files, string folderName, string filePrefix)
    {
        var validFiles = files?.Where(file => file != null && file.Length > 0).ToList() ?? new List<IFormFile>();
        return SaveFilesInternalAsync(validFiles, folderName, filePrefix);
    }

    public void DeleteByUrl(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return;

        var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(GetWebRootPath(), relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public void DeleteMany(IEnumerable<string>? relativeUrls)
    {
        foreach (var relativeUrl in relativeUrls ?? Enumerable.Empty<string>())
        {
            DeleteByUrl(relativeUrl);
        }
    }

    private async Task<List<string>> SaveFilesInternalAsync(IReadOnlyList<IFormFile> files, string folderName, string filePrefix)
    {
        if (files.Count == 0)
            return new List<string>();

        var relativeDirectory = Path.Combine("uploads", folderName);
        var physicalDirectory = Path.Combine(GetWebRootPath(), relativeDirectory);
        Directory.CreateDirectory(physicalDirectory);

        var savedUrls = new List<string>();

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                throw new ArgumentException("Solo se permiten archivos de imagen JPG, PNG, WEBP o GIF.");

            var safePrefix = string.Join('-', filePrefix.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))
                .Replace(' ', '-');
            var fileName = $"{safePrefix}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var physicalPath = Path.Combine(physicalDirectory, fileName);

            await using var stream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(stream);

            savedUrls.Add($"/{relativeDirectory.Replace(Path.DirectorySeparatorChar, '/')}/{fileName}");
        }

        return savedUrls;
    }

    private string GetWebRootPath()
    {
        var webRoot = _environment.WebRootPath;
        if (!string.IsNullOrWhiteSpace(webRoot))
            return webRoot;

        var fallbackPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        Directory.CreateDirectory(fallbackPath);
        return fallbackPath;
    }
}