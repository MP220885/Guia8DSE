using LibrosAPI.Controllers;
using LibrosAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibrosAPI.Tests;

public class LibrosControllerTests
{
    [Fact]
    public async Task PostLibro_AgregaLibro_CuandoLibroEsValido()
    {
        var controller = new LibrosController(Setup.GetDatabaseContext());
        var result = await controller.PostLibro(new Libro { Titulo = "Libro de prueba", Autor = "Autor de prueba", AnioPublicacion = 2026 });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var libro = Assert.IsType<Libro>(created.Value);
        Assert.Equal("Libro de prueba", libro.Titulo);
    }

    [Fact]
    public async Task GetLibro_RetornaLibro_CuandoIdEsValido()
    {
        var context = Setup.GetDatabaseContext();
        var libro = new Libro { Titulo = "Libro de prueba", Autor = "Autor", AnioPublicacion = 2026 };
        context.Libros.Add(libro);
        await context.SaveChangesAsync();

        var result = await new LibrosController(context).GetLibro(libro.Id);
        Assert.Equal("Libro de prueba", Assert.IsType<Libro>(result.Value).Titulo);
    }

    [Fact]
    public async Task GetLibro_RetornaNotFound_CuandoIdNoExiste()
    {
        var result = await new LibrosController(Setup.GetDatabaseContext()).GetLibro(999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostLibro_NoAgregaLibro_CuandoNoTieneTitulo()
    {
        var result = await new LibrosController(Setup.GetDatabaseContext())
            .PostLibro(new Libro { Titulo = "", Autor = "Autor", AnioPublicacion = 2026 });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostLibro_IncrementaConteo_CuandoSeAgregaNuevoLibro()
    {
        var context = Setup.GetDatabaseContext();
        var controller = new LibrosController(context);
        var conteoInicial = context.Libros.Count();
        await controller.PostLibro(new Libro { Titulo = "Libro 1", Autor = "Autor", AnioPublicacion = 2026 });
        await controller.PostLibro(new Libro { Titulo = "Libro 2", Autor = "Autor", AnioPublicacion = 2026 });
        Assert.Equal(conteoInicial + 2, context.Libros.Count());
    }
}
