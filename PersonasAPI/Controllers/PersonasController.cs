using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonasAPI.Models;

namespace PersonasAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonasController(PersonasDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Persona>>> GetPersonas() =>
        await context.Personas.AsNoTracking().ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Persona>> GetPersona(int id)
    {
        var persona = await context.Personas.FindAsync(id);
        return persona is null ? NotFound() : persona;
    }

    [HttpPost]
    public async Task<ActionResult<Persona>> PostPersona(Persona persona)
    {
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(persona, new ValidationContext(persona), validationResults, true);
        if (!isValid || !persona.FechaNacimiento.HasValue || persona.FechaNacimiento.Value.Date > DateTime.Today)
            return BadRequest(validationResults);

        context.Personas.Add(persona);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPersona), new { id = persona.Id }, persona);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutPersona(int id, Persona persona)
    {
        if (id != persona.Id) return BadRequest();
        context.Entry(persona).State = EntityState.Modified;
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.Personas.AnyAsync(p => p.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePersona(int id)
    {
        var persona = await context.Personas.FindAsync(id);
        if (persona is null) return NotFound();
        context.Personas.Remove(persona);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
