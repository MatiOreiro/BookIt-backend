using System.Text;
using BookIt.API.Data;
using BookIt.API.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookIt.API.Infrastructure;

public static class Bootstrapper
{
    public static void LoadDotEnvIfExists(string path)
    {
        if (!File.Exists(path))
            return;

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#') || !line.Contains('='))
                continue;

            var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }

    public static async Task<string?> ResolveConnectionStringAsync(WebApplicationBuilder builder)
    {
        var candidates = new List<string?>();

        if (builder.Environment.IsDevelopment())
        {
            candidates.Add(builder.Configuration.GetConnectionString("LocalConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__LocalConnection"));
        }

        candidates.Add(builder.Configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"));
        candidates.Add(Environment.GetEnvironmentVariable("DATABASE_URL"));

        foreach (var candidate in candidates.Where(candidate => !string.IsNullOrWhiteSpace(candidate)))
        {
            if (await CanConnectAsync(candidate!))
                return candidate;
        }

        return candidates.FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate));
    }

    public static async Task SeedFixedEventCategoriesAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Always apply pending migrations. Skipping when schema already exists
        // leaves production/staging databases without newly added columns.
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.DuplicateTable)
        {
            // Another startup instance already created the schema.
        }

        var fixedCategories = new[]
        {
            "Boda",
            "Cumpleaños de XV",
            "Cumpleaños",
            "Evento corporativo",
            "Bautismo",
            "Graduación",
            "Baile"
        };

        var existingCategories = await context.EventCategories
            .Select(category => category.Nombre)
            .ToListAsync();

        var categoriesToAdd = fixedCategories
            .Where(categoryName => !existingCategories.Contains(categoryName))
            .Select(categoryName => new EventCategory
            {
                Nombre = categoryName,
                FechaCreacion = DateTime.UtcNow
            })
            .ToList();

        if (categoriesToAdd.Count == 0)
            return;

        context.EventCategories.AddRange(categoriesToAdd);
        await context.SaveChangesAsync();
    }

    private static async Task<bool> CanConnectAsync(string connectionString)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

}
