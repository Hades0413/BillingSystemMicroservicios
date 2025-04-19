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

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var montoTotal = detallesVenta.Sum(vp => vp.Cantidad * vp.PrecioUnitario);
            var montoImpuesto = montoTotal * 0.18m;

            var tipoComprobante = await _dbContext.TipoComprobante
                .FirstOrDefaultAsync(tc => tc.TipoComprobanteId == tipoComprobanteId);

            if (tipoComprobante == null)
                return new ResultadoVenta
                {
                    Success = false,
                    Mensaje = "Tipo de comprobante no válido o no encontrado."
                };

            var controlNumeracion = await _dbContext.ControlNumeracion
                .FirstOrDefaultAsync(cn => cn.TipoComprobanteId == tipoComprobanteId);

            if (controlNumeracion == null)
                return new ResultadoVenta
                {
                    Success = false,
                    Mensaje =
                        $"No se encontró la numeración para el tipo de comprobante '{tipoComprobante.TipoComprobanteNombre}'."
                };

            var ultimaVentaUsuario = await _dbContext.Ventas
                .Where(v => v.UsuarioId == usuarioId)
                .OrderByDescending(v => v.VentaId)
                .FirstOrDefaultAsync();

            var numeracionVenta = ultimaVentaUsuario != null
                ? int.Parse(ultimaVentaUsuario.VentaVenta.Split('-')[1]) + 1
                : 1;

            var ultimaVentaPorUsuarioYTipo = await _dbContext.Ventas
                .Where(v => v.UsuarioId == usuarioId && v.TipoComprobanteId == tipoComprobanteId)
                .OrderByDescending(v => v.VentaId)
                .FirstOrDefaultAsync();

            var numeracionCodigo = ultimaVentaPorUsuarioYTipo != null
                ? int.Parse(ultimaVentaPorUsuarioYTipo.VentaCodigo.Split('-')[1]) + 1
                : 1;

            var codigoVenta = $"{controlNumeracion.Prefijo}-{numeracionCodigo:D6}";
            var ventaVenta = $"VEN01-{numeracionVenta:D5}";

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
                VentaCodigo = codigoVenta,
                VentaVenta = ventaVenta
            };

            controlNumeracion.Numeracion++;
            _dbContext.Ventas.Add(venta);
            await _dbContext.SaveChangesAsync();

            _dbContext.ControlNumeracion.Update(controlNumeracion);
            await _dbContext.SaveChangesAsync();

            foreach (var detalle in detallesVenta)
            {
                var ventaProducto = new VentaProducto
                {
                    VentaId = venta.VentaId,
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Total = detalle.Cantidad * detalle.PrecioUnitario
                };
                _dbContext.VentaProductos.Add(ventaProducto);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultadoVenta
            {
                Success = true,
                VentaId = venta.VentaId,
                Mensaje =
                    $"Venta registrada con éxito. Código de venta: {codigoVenta}. Código de venta (VEN01): {ventaVenta}."
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResultadoVenta
            {
                Success = false,
                Mensaje = "Ocurrió un error inesperado. Intente nuevamente o contacte al soporte."
            };
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