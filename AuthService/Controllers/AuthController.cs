using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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

        public AuthController(AuthenticationService authService, AuthDbContext context, OAuthService oauthService,
            JwtService jwtService)
        {
            _authService = authService;
            _context = context;
            _oauthService = oauthService;
            _jwtService = jwtService;
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

        [HttpPost("oauth2-login")]
        public async Task<IActionResult> OAuth2Login([FromBody] AuthServiceOAuthRequest oauthRequest)
        {
            if (!await _oauthService.ValidateOAuthToken(oauthRequest.Token))
            {
                return Unauthorized(new ErrorResponse("OAuth token inválido."));
            }

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.UsuarioCorreo == oauthRequest.Email);
            if (usuario == null)
            {
                return BadRequest(new ErrorResponse("Usuario no encontrado."));
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