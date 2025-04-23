using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Services
{
    /// <summary>
    /// Servicio para manejar la lógica de reenvío de solicitudes a otros servicios a través del API Gateway.
    /// </summary>
    public class ApiGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiGatewayService> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del servicio <see cref="ApiGatewayService"/>.
        /// </summary>
        /// <param name="logger">Proveedor de servicios de registro para realizar un seguimiento de las solicitudes y errores.</param>
        /// <param name="httpClient">Cliente HTTP utilizado para realizar solicitudes a otros servicios.</param>
        public ApiGatewayService(ILogger<ApiGatewayService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Reenvía una solicitud a la URL proporcionada y devuelve el contenido de la respuesta.
        /// </summary>
        /// <param name="url">La URL a la que se debe reenviar la solicitud.</param>
        /// <returns>El contenido de la respuesta recibida desde la URL de destino.</returns>
        /// <exception cref="Exception">Lanza una excepción si no se puede conectar al servicio o si ocurre un error inesperado.</exception>
        public async Task<string> ForwardRequestAsync(string url)
        {
            try
            {
                _logger.LogInformation($"Redirigiendo solicitud a: {url}");

                // Realiza la solicitud HTTP GET a la URL proporcionada
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Lanza una excepción si el código de estado no es 2xx

                // Lee el contenido de la respuesta
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (HttpRequestException ex)
            {
                // Maneja excepciones específicas relacionadas con problemas de conexión
                _logger.LogError($"Error al conectar con {url}: {ex.Message}");
                throw new Exception("El servicio no está disponible en este momento.");
            }
            catch (Exception ex)
            {
                // Maneja errores generales
                _logger.LogError($"Error inesperado en el API Gateway: {ex.Message}");
                throw new Exception("Se produjo un error inesperado en el API Gateway.");
            }
        }
    }
}
