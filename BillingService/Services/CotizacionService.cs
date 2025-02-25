using BillingService.Data;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public class CotizacionService
{
    private readonly CotizacionDBContext _dbContext;

    public CotizacionService(CotizacionDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ResultadoCotizacion> RegistrarCotizacionAsync(
        int usuarioId,
        int empresaId,
        int clienteId,
        DateTime cotizacionFecha,
        decimal cotizacionMontoTotal,
        decimal cotizacionMontoDescuento,
        decimal cotizacionMontoImpuesto,
        List<CotizacionProducto> productos)
    {
        if (usuarioId <= 0 || empresaId <= 0 || clienteId <= 0 || productos == null || productos.Count == 0)
            return new ResultadoCotizacion
            {
                Success = false,
                Mensaje = "Datos de entrada no válidos. Por favor, revise los datos e intente nuevamente."
            };

        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var cotizacion = new Cotizacion
                {
                    UsuarioId = usuarioId,
                    EmpresaId = empresaId,
                    ClienteId = clienteId,
                    CotizacionFecha = cotizacionFecha,
                    CotizacionMontoTotal = cotizacionMontoTotal,
                    CotizacionMontoDescuento = cotizacionMontoDescuento,
                    CotizacionMontoImpuesto = cotizacionMontoImpuesto
                };

                _dbContext.Cotizaciones.Add(cotizacion);
                await _dbContext.SaveChangesAsync();

                var cotizacionId = cotizacion.CotizacionId;
                var prefijo = "CT01";
                var numeracion = await _dbContext.Cotizaciones.CountAsync() + 1;
                var cotizacionCodigo = $"{prefijo}-{numeracion:D6}";
                cotizacion.CotizacionCodigo = cotizacionCodigo;

                _dbContext.Cotizaciones.Update(cotizacion);
                await _dbContext.SaveChangesAsync();

                foreach (var producto in productos)
                {
                    producto.CalcularTotal();

                    var cotizacionProducto = new CotizacionProducto
                    {
                        CotizacionId = cotizacion.CotizacionId,
                        ProductoId = producto.ProductoId,
                        Cantidad = producto.Cantidad,
                        PrecioUnitario = producto.PrecioUnitario,
                        Total = producto.Total
                    };

                    _dbContext.CotizacionProductos.Add(cotizacionProducto);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResultadoCotizacion
                {
                    Success = true,
                    CotizacionId = cotizacion.CotizacionId,
                    Mensaje = $"Cotización registrada con éxito. Código de cotización: {cotizacionCodigo}"
                };
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                return new ResultadoCotizacion
                {
                    Success = false,
                    Mensaje =
                        "Error al registrar la cotización en la base de datos. Verifique la información y los datos ingresados."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResultadoCotizacion
                {
                    Success = false,
                    Mensaje =
                        "Ocurrió un error inesperado. Por favor, intente nuevamente. Si el problema persiste, contacte al soporte."
                };
            }
        }
    }

    public async Task<List<Cotizacion>> ListarCotizacionPorUsuarioAsync(int usuarioId)
    {
        if (usuarioId <= 0) throw new ArgumentException("El ID de usuario no es válido.");

        var cotizaciones = await _dbContext.Cotizaciones
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.CotizacionFecha)
            .ToListAsync();

        return cotizaciones;
    }
}