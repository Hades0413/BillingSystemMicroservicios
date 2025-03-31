using Microsoft.AspNetCore.WebUtilities;

namespace ApiGateway.Services
{
    public class ApiGatewayService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiGatewayService> _logger;

        public ApiGatewayService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ApiGatewayService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> ValidateTokenAsync(string token)
        {
            try
            {
                var authServiceUrl = _configuration["Microservices:AuthService"] + "/api/auth/validate-token";
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await client.GetAsync(authServiceUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error al llamar a AuthService para validar el token: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error de red al llamar a AuthService: {ex.Message}");
                return null;
            }
        }

      public async Task<string> GetAccessTokenAsync(string code)
        {
            try
            {
                var clientId = _configuration["OAuth2:ClientId"];
                var clientSecret = _configuration["OAuth2:ClientSecret"];
                var redirectUri = _configuration["OAuth2:RedirectUri"];
                var tokenUrl = _configuration["OAuth2:Authority"] + "/access_token";

                var client = _httpClientFactory.CreateClient();

                var tokenResponse = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "code", code },
                    { "redirect_uri", redirectUri }
                }));

                var responseString = await tokenResponse.Content.ReadAsStringAsync();
                var queryParams = QueryHelpers.ParseQuery(responseString);
                var accessToken = queryParams["access_token"].ToString();

                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("No se pudo obtener el token de acceso de GitHub.");
                    return null;
                }

                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener el token de GitHub: {ex.Message}");
                return null;
            }
        }
    }
}
