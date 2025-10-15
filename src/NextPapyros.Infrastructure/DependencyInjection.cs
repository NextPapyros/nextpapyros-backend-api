using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NextPapyros.Application.Reports;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Auth;
using NextPapyros.Infrastructure.Persistence;
using NextPapyros.Infrastructure.Reports;
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

        // Repositories section
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IRecepcionRepository, RecepcionRepository>();
        services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
        services.AddScoped<IReporteRepository, ReporteRepository>();
        services.AddScoped<IReportExporter, CsvReportExporter>();
        services.AddScoped<IReportExporter, PdfReportExporter>();

        // Auth section
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}