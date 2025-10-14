// Seeder created in order to have an initial Admin user and role when the app runs for the first time
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Auth;

namespace NextPapyros.API.Startup;

public static class DbSeeder
{
    public static async Task SeedAsync(
        IUsuarioRepository users,
        IRoleRepository roles,
        IPasswordHasher hasher,
        CancellationToken ct = default)
    {
        var adminRole = await roles.GetByNameAsync("Admin", ct);
        if (adminRole is null)
        {
            adminRole = new Rol { Nombre = "Admin", Descripcion = "Administrador" };

            var admin = new Usuario
            {
                // Mocked information used for the initial admin user
                Nombre = "Administrador",
                Email = "admin@admin.com",
                PasswordHash = hasher.Hash("Admin2025*"),
                Activo = true
            };
            admin.Roles.Add(new UsuarioRol { Rol = adminRole, Usuario = admin });

            await users.AddAsync(admin, ct);
            await users.SaveChangesAsync(ct);
        }
    }
}