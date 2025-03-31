using BillingService.Models;
using BillingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CotizacionController : ControllerBase
{
    private readonly CotizacionService _cotizacionService;

    public CotizacionController(CotizacionService cotizacionService)
    {
        _cotizacionService = cotizacionService;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> RegistrarCotizacion([FromBody] CotizacionRequest cotizacionRequest)
    {
        if (cotizacionRequest == null) return BadRequest("La solicitud de cotización no puede ser nula.");

        if (cotizacionRequest.Productos == null || cotizacionRequest.Productos.Count == 0)
            return BadRequest("Los productos de la cotización son obligatorios.");

        try
        {
            var resultado = await _cotizacionService.RegistrarCotizacionAsync(
                cotizacionRequest.UsuarioId,
                cotizacionRequest.EmpresaId,
                cotizacionRequest.ClienteId,
                cotizacionRequest.CotizacionFecha,
                cotizacionRequest.CotizacionMontoTotal,
                cotizacionRequest.CotizacionMontoDescuento,
                cotizacionRequest.CotizacionMontoImpuesto,
                cotizacionRequest.Productos
            );

            if (!resultado.Success)
                return StatusCode(500,
                    $"Error al registrar la cotización: {resultado.Mensaje}. Por favor, intente nuevamente.");

            return Ok($"Cotización registrada con éxito. ID de cotización: {resultado.CotizacionId}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}. Por favor, contacte al soporte.");
        }
    }

    [HttpGet("listar-por-usuario/{usuarioId}")]
    public async Task<IActionResult> ListarCotizacionesPorUsuario(int usuarioId)
    {
        try
        {
            var cotizaciones = await _cotizacionService.ListarCotizacionPorUsuarioAsync(usuarioId);

            if (cotizaciones == null || cotizaciones.Count == 0)
            {
                return NotFound(new { code = 404, message = $"El usuario actual no tiene cotizaciones, por favor realice una nueva cotización." });
            }

            return Ok(new { code = 200, message = "Cotizaciones obtenidas correctamente.", data = cotizaciones });
        }
        catch (Exception ex)
        {
            return BadRequest(new { code = 400, message = $"Ocurrió un error: {ex.Message}" });
        }
    }

}
