using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonasAPI.Controllers;
using PersonasAPI.Models;

namespace PersonasAPI.Tests;

public class PersonasControllerTests
{
    private static PersonasDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PersonasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PersonasDbContext(options);
    }

    private static Persona PersonaValida() => new()
    {
        PrimerNombre = "Ana", PrimerApellido = "López", Dui = "01234567-8",
        FechaNacimiento = new DateTime(2000, 1, 1)
    };

    [Fact]
    public async Task PostPersona_CreaPersona_CuandoDatosSonValidos()
    {
        var result = await new PersonasController(CreateContext()).PostPersona(PersonaValida());
        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task PostPersona_RechazaPersona_CuandoPrimerNombreEsVacio()
    {
        var persona = PersonaValida();
        persona.PrimerNombre = "";
        var result = await new PersonasController(CreateContext()).PostPersona(persona);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostPersona_RechazaPersona_CuandoDuiNoTieneFormatoValido()
    {
        var persona = PersonaValida();
        persona.Dui = "123";
        var result = await new PersonasController(CreateContext()).PostPersona(persona);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostPersona_RechazaPersona_CuandoFechaNacimientoEsNula()
    {
        var persona = PersonaValida();
        persona.FechaNacimiento = null;
        var result = await new PersonasController(CreateContext()).PostPersona(persona);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostPersona_RechazaPersona_CuandoNombreSupera100Caracteres()
    {
        var persona = PersonaValida();
        persona.PrimerNombre = new string('A', 101);
        var result = await new PersonasController(CreateContext()).PostPersona(persona);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
