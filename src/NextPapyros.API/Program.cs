using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NextPapyros.API.Startup;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure;
using NextPapyros.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

// Infra
builder.Services.AddInfrastructure(builder.Configuration);

// JWT
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policyBuilder =>
    {
        var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173")
            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        policyBuilder.WithOrigins(allowedOrigins)
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NextPapyros API",
        Version = "v1",
        Description = "Backend API de NextPapyros"
    });

    // XML comments
    var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Usa el esquema **Bearer**. Ej: `Bearer eyJ...`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// Migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<NextPapyros.Infrastructure.Persistence.NextPapyrosDbContext>();
        
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al migrar la base de datos.");
    }
}

// Middleware Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NextPapyros API v1");
    c.RoutePrefix = "swagger"; // http://localhost:XXXX/swagger
});

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    await DbSeeder.SeedAsync(
        sp.GetRequiredService<IUsuarioRepository>(),
        sp.GetRequiredService<IRoleRepository>(),
        sp.GetRequiredService<IPasswordHasher>(),
        sp.GetRequiredService<IConfiguration>()
    );
}

app.UseHttpsRedirection();

// CORS
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "OK");
app.Run();
