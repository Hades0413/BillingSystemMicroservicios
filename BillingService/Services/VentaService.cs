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
            decimal montoTotal = detallesVenta.Sum(vp => vp.Cantidad * vp.PrecioUnitario); 
            decimal montoImpuesto = montoTotal * 0.18m;

            var tipoComprobante = await _dbContext.TipoComprobante
                .Where(tc => tc.TipoComprobanteId == tipoComprobanteId)
                .FirstOrDefaultAsync();

            if (tipoComprobante == null)
            {
                return new ResultadoVenta
                {
                    Success = false,
                    Mensaje = "Tipo de comprobante no válido o no encontrado. Verifique el tipo de comprobante e intente nuevamente."
                };
            }

            var tipoComprobanteNombre = tipoComprobante.TipoComprobanteNombre;

            var controlNumeracion = await _dbContext.ControlNumeracion
                .Where(cn => cn.TipoComprobanteId == tipoComprobanteId)
                .FirstOrDefaultAsync();

            if (controlNumeracion == null)
            {
                return new ResultadoVenta
                {
                    Success = false,
                    Mensaje = $"No se encontró la numeración para el tipo de comprobante '{tipoComprobanteNombre}'. Por favor, intente nuevamente."
                };
            }

            string codigoVenta = $"{controlNumeracion.Prefijo}-{controlNumeracion.Numeracion:D6}";

            var venta = new Venta
            {
                UsuarioId = usuarioId,
                EmpresaId = empresaId,
                ClienteId = clienteId,
                TipoComprobanteId = tipoComprobanteId,
                VentaFormaPago = formaPago,
                VentaFecha = DateTime.Now,
                VentaMontoTotal = montoTotal,
                VentaMontoDescuento = 0,
                VentaMontoImpuesto = montoImpuesto,
                VentaRucCliente = clienteRuc,
                VentaCodigo = codigoVenta 
            };

            controlNumeracion.Numeracion++;

            _dbContext.Ventas.Add(venta);
            await _dbContext.SaveChangesAsync();

            _dbContext.ControlNumeracion.Update(controlNumeracion);
            await _dbContext.SaveChangesAsync();

            foreach (var detalle in detallesVenta)
            {
                detalle.Total = detalle.Cantidad * detalle.PrecioUnitario;

                var ventaProducto = new VentaProducto
                {
                    VentaId = venta.VentaId,
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Total = detalle.Total 
                };

                _dbContext.VentaProductos.Add(ventaProducto);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultadoVenta
            {
                Success = true,
                VentaId = venta.VentaId,
                Mensaje = $"Venta registrada con éxito. Código de venta: {codigoVenta}."
            };
        }
        catch (DbUpdateException dbEx)
        {
            await transaction.RollbackAsync();
            return new ResultadoVenta
            {
                Success = false,
                Mensaje = "Error al registrar la venta en la base de datos. Verifique la información y los datos ingresados."
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResultadoVenta
            {
                Success = false,
                Mensaje = "Ocurrió un error inesperado. Por favor, intente nuevamente. Si el problema persiste, contacte al soporte."
            };
        }
    }
}

    }
}
