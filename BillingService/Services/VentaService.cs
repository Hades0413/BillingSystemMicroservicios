using BillingService.Data;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public class VentaService
{
    private readonly VentaDBContext _dbContext;

    public VentaService(VentaDBContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<ResultadoVenta> RegistrarVentaAsync(int usuarioId, int empresaId, int clienteId,
        int tipoComprobanteId, string formaPago, List<VentaProducto> detallesVenta, string clienteRuc)
{
    if (usuarioId <= 0 || empresaId <= 0 || clienteId <= 0 || tipoComprobanteId <= 0 || detallesVenta == null ||
        detallesVenta.Count == 0)
        return new ResultadoVenta
        {
            Success = false,
            Mensaje = "Datos de entrada no válidos. Por favor, revise los datos e intente nuevamente."
        };

    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
    {
        try
        {
            var montoTotal = detallesVenta.Sum(vp => vp.Cantidad * vp.PrecioUnitario);
            var montoImpuesto = montoTotal * 0.18m;

            var tipoComprobante = await _dbContext.TipoComprobante
                .Where(tc => tc.TipoComprobanteId == tipoComprobanteId)
                .FirstOrDefaultAsync();

            if (tipoComprobante == null)
                return new ResultadoVenta
                {
                    Success = false,
                    Mensaje =
                        "Tipo de comprobante no válido o no encontrado. Verifique el tipo de comprobante e intente nuevamente."
                };

            var tipoComprobanteNombre = tipoComprobante.TipoComprobanteNombre;

            // Buscar el control de numeración para el tipo de comprobante
            var controlNumeracion = await _dbContext.ControlNumeracion
                .Where(cn => cn.TipoComprobanteId == tipoComprobanteId)
                .FirstOrDefaultAsync();

            if (controlNumeracion == null)
                return new ResultadoVenta
                {
                    Success = false,
                    Mensaje =
                        $"No se encontró la numeración para el tipo de comprobante '{tipoComprobanteNombre}'. Por favor, intente nuevamente."
                };

            // Obtener el último número de venta para ese usuario
            var ultimaVentaUsuario = await _dbContext.Ventas
                .Where(v => v.UsuarioId == usuarioId && v.TipoComprobanteId == tipoComprobanteId)
                .OrderByDescending(v => v.VentaId)
                .FirstOrDefaultAsync();

            // Si ya existen ventas, incrementar el número basado en la última venta
            int numeracionVenta = 1;
            if (ultimaVentaUsuario != null)
            {
                // Extraer la numeración de la última venta (ej: VEN01-00005 -> 5)
                var ultimaNumeracion = ultimaVentaUsuario.VentaVenta.Split('-')[1];
                numeracionVenta = int.Parse(ultimaNumeracion) + 1;
            }

            // Generar el código de venta de la siguiente forma:
            var codigoVenta = $"{controlNumeracion.Prefijo}-{controlNumeracion.Numeracion:D6}";

            // Generar la venta_venta con el prefijo VEN01 y la numeración secuencial por usuario
            var ventaVenta = $"VEN01 - {numeracionVenta:D5}";

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
                VentaCodigo = codigoVenta,  // El código de venta que ya estaba generando
                VentaVenta = ventaVenta     // El nuevo campo con la numeración secuencial por usuario
            };

            // Incrementar la numeración para la próxima venta
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
                Mensaje = $"Venta registrada con éxito. Código de venta: {codigoVenta}. Código de venta (VEN01): {ventaVenta}."
            };
        }
        catch (DbUpdateException dbEx)
        {
            await transaction.RollbackAsync();
            return new ResultadoVenta
            {
                Success = false,
                Mensaje =
                    "Error al registrar la venta en la base de datos. Verifique la información y los datos ingresados."
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResultadoVenta
            {
                Success = false,
                Mensaje =
                    "Ocurrió un error inesperado. Por favor, intente nuevamente. Si el problema persiste, contacte al soporte."
            };
        }
    }
}




    public async Task<List<Venta>> ObtenerVentasAsync()
    {
        try
        {
            var ventas = await _dbContext.Ventas
                .ToListAsync();

            return ventas;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ocurrió un error al obtener las ventas: {ex.Message}");
        }
    }

    public async Task<List<Venta>> ObtenerVentasPorUsuarioIdAsync(int usuarioId)
    {
        try
        {
            var ventas = await _dbContext.Ventas
                .Where(v => v.UsuarioId == usuarioId).ToListAsync();

            return ventas;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ocurrió un error al obtener las ventas para el usuario {usuarioId}: {ex.Message}");
        }
    }
}