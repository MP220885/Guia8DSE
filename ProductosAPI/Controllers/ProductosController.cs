using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductosAPI.Caching;
using ProductosAPI.Models;

namespace ProductosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController(ProductosDbContext context, IRedisCache cache) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
    {
        const string cacheKey = "productos:list";
        var cached = await cache.GetAsync<List<Producto>>(cacheKey);
        if (cached is not null) return cached;
        var productos = await context.Productos.AsNoTracking().ToListAsync();
        await cache.SetAsync(cacheKey, productos, TimeSpan.FromMinutes(10));
        return productos;
    }

    [HttpGet("{id:int}")] public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        var cacheKey = $"productos:{id}";
        var cached = await cache.GetAsync<Producto>(cacheKey);
        if (cached is not null) return cached;
        var producto = await context.Productos.FindAsync(id);
        if (producto is null) return NotFound();
        await cache.SetAsync(cacheKey, producto, TimeSpan.FromMinutes(10));
        return producto;
    }

    [HttpPost] public async Task<ActionResult<Producto>> PostProducto(Producto producto)
    {
        context.Productos.Add(producto);
        await context.SaveChangesAsync();
        await cache.RemoveAsync("productos:list");
        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:int}")] public async Task<IActionResult> PutProducto(int id, Producto producto)
    {
        if (id != producto.Id) return BadRequest();
        context.Entry(producto).State = EntityState.Modified;
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { if (!await context.Productos.AnyAsync(p => p.Id == id)) return NotFound(); throw; }
        await cache.RemoveAsync($"productos:{id}", "productos:list");
        return NoContent();
    }

    [HttpDelete("{id:int}")] public async Task<IActionResult> DeleteProducto(int id)
    {
        var producto = await context.Productos.FindAsync(id);
        if (producto is null) return NotFound();
        context.Productos.Remove(producto);
        await context.SaveChangesAsync();
        await cache.RemoveAsync($"productos:{id}", "productos:list");
        return NoContent();
    }
}
