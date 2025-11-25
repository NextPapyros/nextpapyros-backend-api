using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NextPapyros.Application.Email;
using NextPapyros.Application.Reports;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Auth;
using NextPapyros.Infrastructure.Email;
using NextPapyros.Infrastructure.Persistence;
using NextPapyros.Infrastructure.Reports;
using NextPapyros.Infrastructure.Repositories;
using Resend;

namespace NextPapyros.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NextPapyrosDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")!));

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

        // Services section
        services.AddScoped<IComprobanteService, ComprobanteService>();
        services.AddScoped<IEmailService, ResendEmailService>();
        services.AddHttpClient<IResend, ResendClient>();
        services.AddOptions();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = configuration["Resend:ApiKey"]!;
        });

        // Auth section
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}