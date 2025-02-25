using BillingService.Models;
using BillingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class VentaController : ControllerBase
{
    private readonly VentaService _ventaService;

    public VentaController(VentaService ventaService)
    {
        _ventaService = ventaService;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> RegistrarVenta([FromBody] VentaRequest ventaRequest)
    {
        if (ventaRequest == null) return BadRequest("La solicitud de venta no puede ser nula.");

        if (ventaRequest.DetallesVenta == null || ventaRequest.DetallesVenta.Count == 0)
            return BadRequest("Los detalles de la venta son obligatorios.");

        if (string.IsNullOrEmpty(ventaRequest.ClienteRuc)) return BadRequest("El RUC del cliente es obligatorio.");

        try
        {
            var resultado = await _ventaService.RegistrarVentaAsync(
                ventaRequest.UsuarioId,
                ventaRequest.EmpresaId,
                ventaRequest.ClienteId,
                ventaRequest.TipoComprobanteId,
                ventaRequest.FormaPago,
                ventaRequest.DetallesVenta,
                ventaRequest.ClienteRuc
            );

            if (!resultado.Success)
                return StatusCode(500,
                    $"Error al registrar la venta: {resultado.Mensaje}. Por favor, intente nuevamente.");

            return Ok($"Venta registrada con éxito. ID de venta: {resultado.VentaId}");
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"Error de validación: {ex.Message}. Verifique los datos de entrada.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest($"Error de operación: {ex.Message}. Verifique los datos de entrada.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}. Por favor, contacte al soporte.");
        }
    }

    [HttpGet("listar")]
    public async Task<IActionResult> ListarVentas()
    {
        try
        {
            var ventas = await _ventaService.ObtenerVentasAsync();

            if (ventas == null || !ventas.Any()) return NotFound("No se encontraron ventas registradas.");

            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}. Por favor, contacte al soporte.");
        }
    }

    [HttpGet("listar-por-usuario/{usuarioId}")]
    public async Task<IActionResult> ListarVentasPorUsuario(int usuarioId)
    {
        try
        {
            if (usuarioId <= 0) return BadRequest("El UsuarioId debe ser un valor mayor que cero.");

            var ventas = await _ventaService.ObtenerVentasPorUsuarioIdAsync(usuarioId);

            if (ventas == null || !ventas.Any())
                return NotFound($"No se encontraron ventas para el usuario con ID {usuarioId}.");

            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}. Por favor, contacte al soporte.");
        }
    }
}