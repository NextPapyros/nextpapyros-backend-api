using Microsoft.EntityFrameworkCore;
using NextPapyros.Domain.Entities;

namespace NextPapyros.Infrastructure.Persistence;

public class NextPapyrosDbContext(DbContextOptions<NextPapyrosDbContext> options) : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<ProductoProveedor> ProductoProveedores => Set<ProductoProveedor>();
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<LineaOrdenCompra> LineasOrdenCompra => Set<LineaOrdenCompra>();
    public DbSet<Recepcion> Recepciones => Set<Recepcion>();
    public DbSet<LineaRecepcion> LineasRecepcion => Set<LineaRecepcion>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<LineaVenta> LineasVenta => Set<LineaVenta>();
    public DbSet<Devolucion> Devoluciones => Set<Devolucion>();
    public DbSet<DevolucionLinea> DevolucionLineas => Set<DevolucionLinea>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<LogOperacion> Logs => Set<LogOperacion>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Producto>(e =>
        {
            e.HasKey(p => p.Codigo);
            e.Property(p => p.Codigo).HasMaxLength(50);
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
            e.Property(p => p.Categoria).HasMaxLength(80);
        });

        model.Entity<ProductoProveedor>(e =>
        {
            e.HasKey(pp => new { pp.ProductoCodigo, pp.ProveedorId });
            e.HasOne(pp => pp.Producto)
               .WithMany(p => p.Proveedores)
               .HasForeignKey(pp => pp.ProductoCodigo);
            e.HasOne(pp => pp.Proveedor)
               .WithMany(p => p.Productos)
               .HasForeignKey(pp => pp.ProveedorId);
        });

        model.Entity<OrdenCompra>()
             .HasOne(o => o.Proveedor)
             .WithMany(p => p.OrdenesEmitidas)
             .HasForeignKey(o => o.ProveedorId);

        model.Entity<LineaOrdenCompra>(e =>
        {
            e.HasOne(l => l.OrdenCompra)
             .WithMany(o => o.Lineas)
             .HasForeignKey(l => l.OrdenCompraId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.Producto)
             .WithMany(p => p.LineasOrdenCompra)
             .HasForeignKey(l => l.ProductoCodigo);
        });

        model.Entity<LineaRecepcion>(e =>
        {
            e.HasOne(l => l.Recepcion)
             .WithMany(r => r.Lineas)
             .HasForeignKey(l => l.RecepcionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.Producto)
             .WithMany(p => p.LineasRecepcion)
             .HasForeignKey(l => l.ProductoCodigo);
        });

        model.Entity<LineaVenta>(e =>
        {
            e.HasOne(l => l.Venta)
             .WithMany(v => v.Lineas)
             .HasForeignKey(l => l.VentaId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.Producto)
             .WithMany(p => p.LineasVenta)
             .HasForeignKey(l => l.ProductoCodigo);
        });

        model.Entity<DevolucionLinea>(e =>
        {
            e.HasOne(l => l.Devolucion)
             .WithMany(d => d.Lineas)
             .HasForeignKey(l => l.DevolucionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.LineaVenta)
             .WithMany()
             .HasForeignKey(l => l.LineaVentaId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        model.Entity<MovimientoInventario>()
             .HasOne(m => m.Producto)
             .WithMany(p => p.Movimientos)
             .HasForeignKey(m => m.ProductoCodigo);

        model.Entity<UsuarioRol>(e =>
        {
            e.HasOne(ur => ur.Usuario)
             .WithMany(u => u.Roles)
             .HasForeignKey(ur => ur.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ur => ur.Rol)
             .WithMany(r => r.Usuarios)
             .HasForeignKey(ur => ur.RolId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<RolPermiso>(e =>
        {
            e.HasOne(rp => rp.Rol)
             .WithMany(r => r.Permisos)
             .HasForeignKey(rp => rp.RolId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(rp => rp.Permiso)
             .WithMany(p => p.Roles)
             .HasForeignKey(rp => rp.PermisoId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<LogOperacion>()
             .HasOne(l => l.Usuario)
             .WithMany(u => u.Logs)
             .HasForeignKey(l => l.UsuarioId)
             .OnDelete(DeleteBehavior.Restrict);

        model.Entity<OrdenCompra>()
            .Property(o => o.Estado)
            .HasConversion<string>()
            .HasMaxLength(20);

        model.Entity<Devolucion>()
            .Property(d => d.Estado)
            .HasConversion<string>()
            .HasMaxLength(20);

        base.OnModelCreating(model);

    }
}