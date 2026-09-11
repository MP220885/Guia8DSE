using LibrosAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrosAPI.Tests;

public static class Setup
{
    public static LibrosDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<LibrosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new LibrosDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
