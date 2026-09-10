using LibrosAPI.Models;
using LibrosAPI.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LibrosController(LibrosDbContext context, IRedisCache cache) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IEnumerable<Libro>>> GetLibros()
    {
        const string cacheKey = "libros:list";
        var cached = await cache.GetAsync<List<Libro>>(cacheKey);
        if (cached is not null) return cached;
        var libros = await context.Libros.AsNoTracking().ToListAsync();
        await cache.SetAsync(cacheKey, libros, TimeSpan.FromMinutes(10));
        return libros;
    }

    [HttpGet("{id:int}")] public async Task<ActionResult<Libro>> GetLibro(int id)
    {
        var cacheKey = $"libros:{id}";
        var cached = await cache.GetAsync<Libro>(cacheKey);
        if (cached is not null) return cached;
        var libro = await context.Libros.FindAsync(id);
        if (libro is null) return NotFound();
        await cache.SetAsync(cacheKey, libro, TimeSpan.FromMinutes(10));
        return libro;
    }

    [HttpPost] public async Task<ActionResult<Libro>> PostLibro(Libro libro)
    {
        context.Libros.Add(libro);
        await context.SaveChangesAsync();
        await cache.RemoveAsync("libros:list");
        return CreatedAtAction(nameof(GetLibro), new { id = libro.Id }, libro);
    }

    [HttpPut("{id:int}")] public async Task<IActionResult> PutLibro(int id, Libro libro)
    {
        if (id != libro.Id) return BadRequest();
        context.Entry(libro).State = EntityState.Modified;
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { if (!await context.Libros.AnyAsync(l => l.Id == id)) return NotFound(); throw; }
        await cache.RemoveAsync($"libros:{id}", "libros:list");
        return NoContent();
    }

    [HttpDelete("{id:int}")] public async Task<IActionResult> DeleteLibro(int id)
    {
        var libro = await context.Libros.FindAsync(id);
        if (libro is null) return NotFound();
        context.Libros.Remove(libro);
        await context.SaveChangesAsync();
        await cache.RemoveAsync($"libros:{id}", "libros:list");
        return NoContent();
    }
}
