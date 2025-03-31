using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiGatewayController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ApiGatewayController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("registrar-venta")]
        [Authorize]
        public async Task<IActionResult> RegistrarVenta([FromBody] JsonElement ventaRequest)
        {
            var client = _httpClientFactory.CreateClient();
            var billingServiceUrl = _configuration["Microservices:BillingService"] + "/api/venta/registrar";

            var content = new StringContent(ventaRequest.ToString(), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(billingServiceUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { message = "Error al registrar la venta" });
            }

            var responseData = await response.Content.ReadAsStringAsync();
            return Ok(new { message = "Venta registrada con éxito", data = responseData });
        }

        [HttpGet("listar-ventas")]
        [Authorize]
        public async Task<IActionResult> ListarVentas()
        {
            var client = _httpClientFactory.CreateClient();
            var billingServiceUrl = _configuration["Microservices:BillingService"] + "/api/venta/listar";

            var response = await client.GetAsync(billingServiceUrl);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { message = "Error al obtener las ventas" });
            }

            var responseData = await response.Content.ReadAsStringAsync();
            return Ok(new { message = "Ventas obtenidas con éxito", data = responseData });
        }
    }
}
