using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[Route("api/gateway")]
[ApiController]
public class ApiGatewayController : ControllerBase
{
    private readonly ApiGatewayService _apiGatewayService;
    private readonly ILogger<ApiGatewayController> _logger;

    public ApiGatewayController(ApiGatewayService apiGatewayService, ILogger<ApiGatewayController> logger)
    {
        _apiGatewayService = apiGatewayService;
        _logger = logger;
    }

    // Endpoint para verificar si el API Gateway está funcionando
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        _logger.LogInformation("Solicitud de verificación de estado recibida.");
        return Ok(new { status = "API Gateway está funcionando correctamente." });
    }

    // Endpoint para manejar errores personalizados y reenviar solicitudes
    [HttpGet("forward")]
    public async Task<IActionResult> ForwardRequest([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            _logger.LogWarning("Solicitud de redirección sin URL.");
            return BadRequest(new { error = "Debe proporcionar una URL válida." });
        }

        try
        {
            var response = await _apiGatewayService.ForwardRequestAsync(url);
            return Ok(new { message = "Solicitud procesada con éxito.", data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error en la solicitud de redirección: {ex.Message}");
            return StatusCode(500, new { error = "Hubo un problema al procesar la solicitud.", details = ex.Message });
        }
    }
}