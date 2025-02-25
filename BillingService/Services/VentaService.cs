using BillingService.Data;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services
{
    public class VentaService
    {
        private readonly VentaDBContext _dbContext;

        public VentaService(VentaDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResultadoVenta> RegistrarVentaAsync(int usuarioId, int empresaId, int clienteId, int tipoComprobanteId, string formaPago, List<VentaProducto> detallesVenta, string clienteRuc)
{
    // Validación de los parámetros de entrada
    if (usuarioId <= 0 || empresaId <= 0 || clienteId <= 0 || tipoComprobanteId <= 0 || detallesVenta == null || detallesVenta.Count == 0)
    {
        return new ResultadoVenta
        {
            Success = false,
            Mensaje = "Datos de entrada no válidos. Por favor, revise los datos e intente nuevamente."
        };
    }

    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
    {
        try
        {
            // Calcular el total de la venta y el monto de impuestos
            decimal montoTotal = detallesVenta.Sum(vp => vp.Cantidad * vp.PrecioUnitario); // Suma de cantidad * precio unitario
            decimal montoImpuesto = montoTotal * 0.18m; // Asumiendo un 18% de impuesto

            var venta = new Venta
            {
                UsuarioId = usuarioId,
                EmpresaId = empresaId,
                ClienteId = clienteId,
                TipoComprobanteId = tipoComprobanteId,
                VentaFormaPago = formaPago,
                VentaFecha = DateTime.Now,
                VentaMontoTotal = montoTotal,
                VentaMontoDescuento = 0, // Ajusta esta lógica si se aplica descuento
                VentaMontoImpuesto = montoImpuesto,
                VentaRucCliente = clienteRuc,
            };

            _dbContext.Ventas.Add(venta);
            await _dbContext.SaveChangesAsync(); // Guardamos la venta para obtener su VentaId

            foreach (var detalle in detallesVenta)
            {
                // Calcular el total del producto (si no se calculó previamente)
                detalle.Total = detalle.Cantidad * detalle.PrecioUnitario;

                var ventaProducto = new VentaProducto
                {
                    VentaId = venta.VentaId,
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Total = detalle.Total // Usamos el PrecioTotal calculado para el detalle
                };

                _dbContext.VentaProductos.Add(ventaProducto);
            }

            await _dbContext.SaveChangesAsync(); // Guardamos los productos de la venta
            await transaction.CommitAsync();

            // Retornar ResultadoVenta con éxito y datos
            return new ResultadoVenta
            {
                Success = true,
                VentaId = venta.VentaId,
                Mensaje = "Venta registrada con éxito."
            };
        }
        catch (DbUpdateException dbEx)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Database Error: {dbEx.Message}");
            return new ResultadoVenta
            {
                Success = false,
                Mensaje = "Error al registrar la venta en la base de datos. Por favor, intente nuevamente."
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"General Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return new ResultadoVenta
            {
                Success = false,
                Mensaje = "Ocurrió un error inesperado. Por favor, intente nuevamente."
            };
        }
    }
}

        }
    }

