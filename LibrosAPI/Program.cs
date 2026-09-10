using LibrosAPI.Models;
using LibrosAPI.Caching;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LibrosDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
if (string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddSingleton<IRedisCache, NoOpRedisCache>();
    builder.Services.AddOutputCache();
}
else
{
    builder.Services.AddStackExchangeRedisOutputCache(options => options.Configuration = redisConnection);
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisConnection, true)));
    builder.Services.AddSingleton<IRedisCache, RedisCache>();
}

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await LibroSeeder.EnsureSeededAsync(
        scope.ServiceProvider.GetRequiredService<LibrosDbContext>(),
        scope.ServiceProvider.GetRequiredService<IRedisCache>(),
        app.Environment.ContentRootPath);
}
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
app.UseOutputCache();
app.MapControllers();
app.Run();
