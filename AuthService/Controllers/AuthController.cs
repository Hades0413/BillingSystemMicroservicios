using AuthService.Data;
using AuthService.Models;
using AuthService.Response;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AuthService.Controllers
{
    /// <summary>
    /// Controlador para manejar la autenticación de usuarios.
    /// Proporciona métodos para login con credenciales, login con OAuth2 (GitHub), y validación de token JWT.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly IConfiguration _configuration;
        private readonly AuthDbContext _context;
        private readonly JwtService _jwtService;
        private readonly OAuthService _oauthService;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de autenticación.
        /// </summary>
        /// <param name="authService">Servicio de autenticación para gestionar el login con credenciales.</param>
        /// <param name="context">Contexto de base de datos para acceder a la información de autenticación.</param>
        /// <param name="oauthService">Servicio para manejar la autenticación OAuth2.</param>
        /// <param name="jwtService">Servicio para generar y validar JWT.</param>
        /// <param name="configuration">Configuración de la aplicación, como las credenciales de OAuth2.</param>
        public AuthController(AuthenticationService authService, AuthDbContext context, OAuthService oauthService,
            JwtService jwtService, IConfiguration configuration)
        {
            _authService = authService;
            _context = context;
            _oauthService = oauthService;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        /// <summary>
        /// Endpoint para realizar el login utilizando credenciales (correo y contraseña).
        /// </summary>
        /// <param name="auth">Objeto que contiene las credenciales del usuario.</param>
        /// <returns>Resultado de la autenticación con un token JWT si las credenciales son correctas.</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] Auth auth)
        {
            try
            {
                // Verifica las credenciales y genera un token JWT
                var token = _authService.Login(auth.Correo, auth.Contrasena);
                if (string.IsNullOrEmpty(token)) return Unauthorized(new ErrorResponse("Credenciales incorrectas."));

                return Ok(new SuccessResponse("Login exitoso", new { token }));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse("Hubo un problema al procesar la solicitud: " + ex.Message));
            }
        }

        /// <summary>
        /// Endpoint para redirigir al usuario a GitHub para autenticación OAuth2.
        /// </summary>
        /// <returns>Redirección al flujo de autorización de GitHub.</returns>
        [HttpGet("oauth2-login")]
        public IActionResult OAuth2Login()
        {
            var clientId = _configuration["OAuth2:ClientId"];
            var redirectUri = _configuration["OAuth2:RedirectUri"];
            var githubAuthUrl =
                $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope=read:user";

            return Redirect(githubAuthUrl);
        }

        /// <summary>
        /// Endpoint para manejar la devolución del código de autorización de GitHub y obtener el token de acceso.
        /// </summary>
        /// <param name="code">El código de autorización proporcionado por GitHub.</param>
        /// <returns>Token JWT generado para el usuario autenticado a través de GitHub.</returns>
        [HttpGet("oauth2-callback")]
        public async Task<IActionResult> OAuth2Callback(string code)
        {
            var clientId = _configuration["OAuth2:ClientId"];
            var clientSecret = _configuration["OAuth2:ClientSecret"];
            var redirectUri = _configuration["OAuth2:RedirectUri"];
            var tokenUrl = "https://github.com/login/oauth/access_token";

            var client = new HttpClient();
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
                return Unauthorized(new ErrorResponse("No se pudo obtener el token de acceso de GitHub."));

            var userInfoUrl = "https://api.github.com/user";
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Add("User-Agent", "AuthService");

            var userInfoResponse = await client.GetStringAsync(userInfoUrl);
            var userInfo = JsonConvert.DeserializeObject<GitHubUser>(userInfoResponse);

            var email = string.IsNullOrEmpty(userInfo?.Email) ? "dato.desconocido@github.com" : userInfo.Email;
            var name = string.IsNullOrEmpty(userInfo?.Name) ? "Dato Desconocido" : userInfo.Name;
            var login = string.IsNullOrEmpty(userInfo?.Login) ? "Dato Desconocido" : userInfo.Login;

            var auth = await _context.Auth
                .Where(u => u.Correo == email)
                .FirstOrDefaultAsync();

            if (auth == null)
            {
                auth = new Auth
                {
                    Correo = email,
                    Contrasena = "ContraseñaTemporal123"
                };

                _context.Auth.Add(auth);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Auth.Update(auth);
                await _context.SaveChangesAsync();
            }

            var token = _jwtService.GenerateJwtToken(auth);
            return Ok(new SuccessResponse("Login exitoso", new { token }));
        }

        /// <summary>
        /// Endpoint para validar el token JWT en una solicitud autenticada.
        /// </summary>
        /// <returns>Resultado de la validación del token.</returns>
        [Authorize]
        [HttpGet("validate-token")]
        public IActionResult ValidateToken()
        {
            try
            {
                return Ok(new SuccessResponse("Token válido."));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ValidateToken: {ex.Message}");
                return StatusCode(500, new ErrorResponse($"Error interno: {ex.Message}"));
            }
        }
    }
}
