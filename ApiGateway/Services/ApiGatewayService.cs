using Microsoft.AspNetCore.WebUtilities;

namespace ApiGateway.Services
{
    public class ApiGatewayService
    {
        private readonly ILogger<ApiGatewayService> _logger;
        private readonly HttpClient _httpClient;

        public ApiGatewayService(ILogger<ApiGatewayService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string> ForwardRequestAsync(string url)
        {
            try
            {
                _logger.LogInformation($"Redirigiendo solicitud a: {url}");
            
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Lanza una excepción si el código de estado no es 2xx

                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error al conectar con {url}: {ex.Message}");
                throw new Exception("El servicio no está disponible en este momento.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado en el API Gateway: {ex.Message}");
                throw new Exception("Se produjo un error inesperado en el API Gateway.");
            }
        }
    }

}
