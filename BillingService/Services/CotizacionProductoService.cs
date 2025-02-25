using BillingService.Data;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public class CotizacionProductoService
{
    private readonly CotizacionDBContext _dbContext;

    public CotizacionProductoService(CotizacionDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CotizacionProducto>> ListarCotizacionProductoPorCotizacionIdAsync(int cotizacionId)
    {
        if (cotizacionId <= 0) throw new ArgumentException("El cotizacionId debe ser mayor que cero.");

        var productos = await _dbContext.CotizacionProductos
            .Where(cp => cp.CotizacionId == cotizacionId)
            .ToListAsync();

        if (productos == null || !productos.Any())
            throw new InvalidOperationException("No se encontraron productos para esta cotización.");

        return productos;
    }
}