using BillingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class VentaProductoController : ControllerBase
{
    private readonly VentaProductoService _ventaProductoService;

    public VentaProductoController(VentaProductoService ventaProductoService)
    {
        _ventaProductoService = ventaProductoService;
    }

    [HttpGet("listar")]
    public async Task<IActionResult> ListarVentaProductos()
    {
        try
        {
            var productos = await _ventaProductoService.ObtenerVentaProductosAsync();
            return Ok(productos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}. Por favor, contacte al soporte.");
        }
    }

    [HttpGet("listar-por-venta/{ventaId}")]
    public async Task<IActionResult> ListarVentaProductosPorVentaId(int ventaId)
    {
        try
        {
            if (ventaId <= 0) return BadRequest("El VentaId debe ser mayor que cero.");

            var productos = await _ventaProductoService.ObtenerVentaProductosPorVentaIdAsync(ventaId);

            if (productos == null || !productos.Any())
                return NotFound($"No se encontraron productos para la venta con ID {ventaId}.");

            return Ok(productos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}. Por favor, contacte al soporte.");
        }
    }
}