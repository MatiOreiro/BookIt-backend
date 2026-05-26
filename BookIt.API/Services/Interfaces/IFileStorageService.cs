using Microsoft.AspNetCore.Http;

namespace BookIt.API.Services.Interfaces;

public interface IFileStorageService
{
    Task<string?> SaveSingleAsync(IFormFile? file, string folderName, string filePrefix);
    Task<List<string>> SaveManyAsync(IEnumerable<IFormFile>? files, string folderName, string filePrefix);
    void DeleteByUrl(string? relativeUrl);
    void DeleteMany(IEnumerable<string>? relativeUrls);
}