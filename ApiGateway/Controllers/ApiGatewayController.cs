using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador para manejar las solicitudes entrantes en el API Gateway.
    /// Proporciona endpoints para verificar el estado del gateway y reenviar solicitudes a otros servicios.
    /// </summary>
    [Route("api/gateway")]
    [ApiController]
    public class ApiGatewayController : ControllerBase
    {
        private readonly ApiGatewayService _apiGatewayService;
        private readonly ILogger<ApiGatewayController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del controlador <see cref="ApiGatewayController"/>.
        /// </summary>
        /// <param name="apiGatewayService">Servicio que maneja la lógica de redirección de solicitudes.</param>
        /// <param name="logger">Proveedor de servicios de registro para realizar un seguimiento de las solicitudes y errores.</param>
        public ApiGatewayController(ApiGatewayService apiGatewayService, ILogger<ApiGatewayController> logger)
        {
            _apiGatewayService = apiGatewayService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint para verificar el estado del API Gateway.
        /// </summary>
        /// <returns>Devuelve un mensaje indicando que el API Gateway está funcionando correctamente.</returns>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            _logger.LogInformation("Solicitud de verificación de estado recibida.");
            return Ok(new { status = "API Gateway está funcionando correctamente." });
        }

        /// <summary>
        /// Endpoint para manejar errores personalizados y reenviar solicitudes a otras URLs.
        /// </summary>
        /// <param name="url">La URL a la que se debe reenviar la solicitud.</param>
        /// <returns>Devuelve un mensaje indicando el estado de la solicitud reenviada.</returns>
        /// <remarks>
        /// Si no se proporciona una URL válida, se devuelve un mensaje de error.
        /// Si ocurre un error al procesar la solicitud, se devuelve un error con detalles.
        /// </remarks>
        [HttpGet("forward")]
        public async Task<IActionResult> ForwardRequest([FromQuery] string url)
        {
            // Verificar que la URL no esté vacía o nula
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogWarning("Solicitud de redirección sin URL.");
                return BadRequest(new { error = "Debe proporcionar una URL válida." });
            }

            try
            {
                // Reenviar la solicitud a la URL proporcionada utilizando el servicio de gateway
                var response = await _apiGatewayService.ForwardRequestAsync(url);
                return Ok(new { message = "Solicitud procesada con éxito.", data = response });
            }
            catch (Exception ex)
            {
                // Registrar el error y devolver un mensaje adecuado al cliente
                _logger.LogError($"Error en la solicitud de redirección: {ex.Message}");
                return StatusCode(500, new { error = "Hubo un problema al procesar la solicitud.", details = ex.Message });
            }
        }
    }
}
