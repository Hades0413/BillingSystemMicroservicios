using BillingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CotizacionProductoController : ControllerBase
{
    private readonly CotizacionProductoService _cotizacionProductoService;

    public CotizacionProductoController(CotizacionProductoService cotizacionProductoService)
    {
        _cotizacionProductoService = cotizacionProductoService;
    }

    [HttpGet("listar-por-cotizacion/{cotizacionId}")]
    public async Task<IActionResult> ListarProductosPorCotizacionId(int cotizacionId)
    {
        try
        {
            var productos = await _cotizacionProductoService.ListarCotizacionProductoPorCotizacionIdAsync(cotizacionId);
            return Ok(productos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"Error de validación: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }
}