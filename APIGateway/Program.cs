using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt:Key.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Falta Jwt:Issuer.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Falta Jwt:Audience.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddOcelot();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.UseWhen(context => context.Request.Path.Equals("/auth/login", StringComparison.OrdinalIgnoreCase), authApp => authApp.Run(async context =>
{
    if (!HttpMethods.IsPost(context.Request.Method))
    {
        context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
        return;
    }

    var request = await context.Request.ReadFromJsonAsync<LoginRequest>();
    if (request is null || request.Username != builder.Configuration["Jwt:Username"] || request.Password != builder.Configuration["Jwt:Password"])
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    var claims = new[] { new Claim(ClaimTypes.Name, request.Username) };
    var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(jwtIssuer, jwtAudience, claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: credentials);

    await context.Response.WriteAsJsonAsync(new { access_token = new JwtSecurityTokenHandler().WriteToken(token), token_type = "Bearer", expires_in = 3600 });
}));

await app.UseOcelot();
await app.RunAsync();

record LoginRequest(string Username, string Password);
