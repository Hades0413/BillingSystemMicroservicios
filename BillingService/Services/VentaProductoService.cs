using BillingService.Data;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public class VentaProductoService
{
    private readonly VentaDBContext _dbContext;

    public VentaProductoService(VentaDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<VentaProducto>> ObtenerVentaProductosAsync()
    {
        try
        {
            return await _dbContext.VentaProductos
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Ocurrió un error al obtener los productos de venta: {ex.Message}");
        }
    }

    public async Task<List<VentaProducto>> ObtenerVentaProductosPorVentaIdAsync(int ventaId)
    {
        try
        {
            return await _dbContext.VentaProductos
                .Where(vp => vp.VentaId == ventaId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Ocurrió un error al obtener los productos de la venta con ID {ventaId}: {ex.Message}");
        }
    }
}