using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Data;

public class FarmaciaDbContext : DbContext
{
    public FarmaciaDbContext(DbContextOptions<FarmaciaDbContext> options) : base(options) { }

    public DbSet<Rol> Roles { get; set; }
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Lote> Lotes { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<DetalleVenta> DetalleVentas { get; set; }
    public DbSet<MetodoPago> MetodosPago { get; set; }
    public DbSet<BitacoraAuditoria> BitacoraAuditoria { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Devolucion> Devoluciones { get; set; }
    public DbSet<DevolucionDetalle> DevolucionDetalles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Empleado - Rol
        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Rol)
            .WithMany(r => r.Empleados)
            .HasForeignKey(e => e.RolID);

        // Producto - Categoria
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.IdCategoria);

        // Producto - Lotes
        modelBuilder.Entity<Lote>()
            .HasOne(l => l.Producto)
            .WithMany(p => p.Lotes)
            .HasForeignKey(l => l.ProductoID);

        // Lote - Proveedor
        modelBuilder.Entity<Lote>()
            .HasOne(l => l.Proveedor)
            .WithMany(p => p.Lotes)
            .HasForeignKey(l => l.ProveedorID);

        // Venta - DetalleVenta
        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Venta)
            .WithMany(v => v.DetalleVentas)
            .HasForeignKey(d => d.VentaID);

        // DetalleVenta - Lote (trazabilidad PEPS)
        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Lote)
            .WithMany()
            .HasForeignKey(d => d.LoteID);

        // DetalleVenta - Producto
        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Producto)
            .WithMany(p => p.DetalleVentas)
            .HasForeignKey(d => d.ProductoID);

        // Venta - Empleado
        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Empleado)
            .WithMany(e => e.Ventas)
            .HasForeignKey(v => v.EmpleadoID);

        // Venta - Cliente
        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Cliente)
            .WithMany(c => c.Ventas)
            .HasForeignKey(v => v.ClienteID);

        // Venta - MetodoPago
        modelBuilder.Entity<Venta>()
            .HasOne(v => v.MetodoPago)
            .WithMany(m => m.Ventas)
            .HasForeignKey(v => v.MetodoPagoID);

        // Bitacora - Empleado
        modelBuilder.Entity<BitacoraAuditoria>()
            .HasOne(b => b.Empleado)
            .WithMany()
            .HasForeignKey(b => b.EmpleadoID);

        // Devolucion - Venta
        modelBuilder.Entity<Devolucion>()
            .HasOne(d => d.Venta)
            .WithMany()
            .HasForeignKey(d => d.VentaID);

        // Devolucion - Empleado (cajero que solicita)
        modelBuilder.Entity<Devolucion>()
            .HasOne(d => d.Empleado)
            .WithMany()
            .HasForeignKey(d => d.EmpleadoID)
            .OnDelete(DeleteBehavior.Restrict);

        // Devolucion - Regente (autoriza)
        modelBuilder.Entity<Devolucion>()
            .HasOne(d => d.Regente)
            .WithMany()
            .HasForeignKey(d => d.RegenteID)
            .OnDelete(DeleteBehavior.Restrict);

        // DevolucionDetalle - Devolucion
        modelBuilder.Entity<DevolucionDetalle>()
            .HasOne(d => d.Devolucion)
            .WithMany(d => d.Detalles)
            .HasForeignKey(d => d.DevolucionID);

        // DevolucionDetalle - DetalleVenta
        modelBuilder.Entity<DevolucionDetalle>()
            .HasOne(d => d.DetalleVenta)
            .WithMany()
            .HasForeignKey(d => d.DetalleVentaID);

        // Indices para rendimiento
        modelBuilder.Entity<Lote>()
            .HasIndex(l => new { l.ProductoID, l.FechaVencimiento });

        modelBuilder.Entity<BitacoraAuditoria>()
            .HasIndex(b => b.FechaHora);

        modelBuilder.Entity<Venta>()
            .HasIndex(v => v.Fecha);

        modelBuilder.Entity<Devolucion>()
            .HasIndex(d => d.Estado);
    }
}