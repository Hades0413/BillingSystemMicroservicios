using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data;

public class VentaDBContext : DbContext
{
    public VentaDBContext(DbContextOptions<VentaDBContext> options) : base(options)
    {
    }

    public DbSet<Venta> Ventas { get; set; }
    public DbSet<VentaProducto> VentaProductos { get; set; }
    public DbSet<ControlNumeracion> ControlNumeracion { get; set; }
    public DbSet<TipoComprobante> TipoComprobante { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VentaProducto>()
            .HasKey(vp => new { vp.VentaId, vp.ProductoId });

        modelBuilder.Entity<Venta>()
            .Property(v => v.VentaMontoDescuento)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Venta>()
            .Property(v => v.VentaMontoImpuesto)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Venta>()
            .Property(v => v.VentaMontoTotal)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<VentaProducto>()
            .Property(vp => vp.Total)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<VentaProducto>()
            .Property(vp => vp.PrecioUnitario)
            .HasColumnType("decimal(18,2)");
    }

    public async Task<(bool success, int ventaId, string mensaje)> RegistrarVentaAsync(
        int usuarioId,
        int empresaId,
        int clienteId,
        int tipoComprobanteId,
        string formaPago,
        List<VentaProducto> detallesVenta,
        string clienteRuc)
    {
        if (usuarioId <= 0 || empresaId <= 0 || clienteId <= 0)
            throw new ArgumentException("Los IDs de usuario, empresa o cliente deben ser mayores que 0.");

        try
        {
            var venta = new Venta
            {
                UsuarioId = usuarioId,
                EmpresaId = empresaId,
                ClienteId = clienteId,
                TipoComprobanteId = tipoComprobanteId,
                VentaFormaPago = formaPago,
                VentaRucCliente = clienteRuc,
                VentaFecha = DateTime.Now,
                VentaMontoDescuento = 0,
                VentaMontoImpuesto = detallesVenta.Sum(vp => vp.Total * 0.18m),
                VentaMontoTotal = detallesVenta.Sum(vp => vp.Total)
            };

            await Ventas.AddAsync(venta);
            await SaveChangesAsync();

            foreach (var detalle in detallesVenta)
            {
                var ventaProducto = new VentaProducto
                {
                    VentaId = venta.VentaId,
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Total = detalle.Total
                };

                await VentaProductos.AddAsync(ventaProducto);
            }

            await SaveChangesAsync();

            return (true, venta.VentaId, "Venta registrada con éxito.");
        }
        catch (Exception ex)
        {
            return (false, 0, $"Ocurrió un error al registrar la venta: {ex.Message}");
        }
    }
}