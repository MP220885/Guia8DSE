using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using LibrosAPI.Caching;

namespace LibrosAPI.Models;

public static class LibroSeeder
{
    public static async Task EnsureSeededAsync(
        LibrosDbContext context,
        IRedisCache cache,
        string contentRootPath)
    {
        await context.Database.EnsureCreatedAsync();

        var generatedBooks = await context.Libros
            .Where(book => book.Titulo.StartsWith("Libro de catálogo"))
            .ToListAsync();
        var generatedBookIds = generatedBooks.Select(book => book.Id).ToArray();
        if (generatedBooks.Count > 0)
        {
            context.Libros.RemoveRange(generatedBooks);
            await context.SaveChangesAsync();
        }

        var dataPath = Path.Combine(contentRootPath, "Data", "libros.mockaroo.json");
        var data = await File.ReadAllTextAsync(dataPath);
        var mockarooBooks = JsonSerializer.Deserialize<List<MockarooLibro>>(data)
            ?? throw new InvalidOperationException("No se pudieron leer los datos de Mockaroo.");

        var existingTitles = await context.Libros.Select(book => book.Titulo).ToListAsync();
        var booksToAdd = mockarooBooks.Where(book => !existingTitles.Contains(book.Titulo)).Select(book => new Libro
            {
                Titulo = book.Titulo,
                Autor = book.Autor,
                AnioPublicacion = book.AnioPublicacion
            });

        context.Libros.AddRange(booksToAdd);
        await context.SaveChangesAsync();
        await cache.RemoveAsync(["libros:list", .. generatedBookIds.Select(id => $"libros:{id}")]);
    }

    private sealed record MockarooLibro(string Titulo, string Autor, int AnioPublicacion);
}
