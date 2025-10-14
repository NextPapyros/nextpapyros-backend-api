using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Auth;
using NextPapyros.Infrastructure.Persistence;
using NextPapyros.Infrastructure.Repositories;

namespace NextPapyros.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NextPapyrosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")!,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "dbo")));

        // Repository section
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Auth section
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}