using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly OAuthService _oauthService;
        private readonly JwtService _jwtService;
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AuthenticationService authService, AuthDbContext context, OAuthService oauthService,
            JwtService jwtService, IConfiguration configuration)
        {
            _authService = authService;
            _context = context;
            _oauthService = oauthService;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var token = _authService.Login(loginRequest.Correo, loginRequest.Contrasena);
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new ErrorResponse("Credenciales incorrectas."));
                }

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

        [HttpGet("oauth2-login")]
        public IActionResult OAuth2Login()
        {
            var clientId = _configuration["OAuth2:ClientId"];
            var redirectUri = _configuration["OAuth2:RedirectUri"];
            var githubAuthUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope=read:user";

            return Redirect(githubAuthUrl);
        }


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
    var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(responseString);
    var accessToken = queryParams["access_token"].ToString();

    if (string.IsNullOrEmpty(accessToken))
    {
        return Unauthorized(new ErrorResponse("No se pudo obtener el token de acceso de GitHub."));
    }

    var userInfoUrl = "https://api.github.com/user";
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
    client.DefaultRequestHeaders.Add("User-Agent", "AuthService");

    var userInfoResponse = await client.GetStringAsync(userInfoUrl);
    var userInfo = JsonConvert.DeserializeObject<GitHubUser>(userInfoResponse);

    string email = string.IsNullOrEmpty(userInfo?.Email) ? "dato.desconocido@github.com" : userInfo.Email;
    string name = string.IsNullOrEmpty(userInfo?.Name) ? "Dato Desconocido" : userInfo.Name;
    string login = string.IsNullOrEmpty(userInfo?.Login) ? "Dato Desconocido" : userInfo.Login;
    
    var usuario = await _context.Usuario
        .Where(u => u.UsuarioCorreo == email)
        .FirstOrDefaultAsync();

    if (usuario == null)
    {
        usuario = new Usuario
        {
            UsuarioCorreo = email,
            UsuarioNombres = name,
            UsuarioApellidos = "Dato Desconocido",
            UsuarioFechaUltimaActualizacion = DateTime.Now,
            UsuarioContrasena = "ContraseñaTemporal123" 
        };

        _context.Usuario.Add(usuario);
        await _context.SaveChangesAsync();
    }
    else
    {
        usuario.UsuarioNombres = name;
        usuario.UsuarioApellidos = "Dato Desconocido"; 
        usuario.UsuarioFechaUltimaActualizacion = DateTime.Now;

        _context.Usuario.Update(usuario);
        await _context.SaveChangesAsync();
    }

    var token = _jwtService.GenerateJwtToken(usuario);
    return Ok(new SuccessResponse("Login exitoso", new { token }));
}


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
