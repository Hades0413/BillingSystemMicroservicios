using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data;

public class CotizacionDBContext : DbContext
{
    public CotizacionDBContext(DbContextOptions<CotizacionDBContext> options) : base(options)
    {
    }

    public DbSet<Cotizacion> Cotizaciones { get; set; }
    public DbSet<CotizacionProducto> CotizacionProductos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cotizacion>().ToTable("Cotizacion");
        modelBuilder.Entity<CotizacionProducto>().ToTable("CotizacionProductos");
    }
}