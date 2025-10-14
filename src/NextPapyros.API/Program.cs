using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NextPapyros.API.Startup;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Auth;
using NextPapyros.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure section
builder.Services.AddInfrastructure(builder.Configuration);

// Authentication JWT section
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Seeder added to create an initial Admin user and role. Won't be created if they already exist
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    await DbSeeder.SeedAsync(
        sp.GetRequiredService<IUsuarioRepository>(),
        sp.GetRequiredService<IRoleRepository>(),
        sp.GetRequiredService<IPasswordHasher>()
    );
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "OK");
app.Run();
