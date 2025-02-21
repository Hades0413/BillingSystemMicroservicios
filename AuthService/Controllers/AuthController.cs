using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;

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

        public AuthController(AuthenticationService authService, AuthDbContext context, OAuthService oauthService, JwtService jwtService)
        {
            _authService = authService;
            _context = context;
            _oauthService = oauthService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            var token = _authService.Login(loginRequest.Correo, loginRequest.Contrasena);
            if (token == null)
            {
                var errorResponse = new ErrorResponse("Credenciales incorrectas.");
                return Unauthorized(errorResponse);
            }

            var successResponse = new SuccessResponse("Login exitoso", new { token });
            return Ok(successResponse);
        }

        [HttpPost("oauth2-login")]
        public async Task<IActionResult> OAuth2Login([FromBody] AuthServiceOAuthRequest oauthRequest) 
        {
            var isValid = await _oauthService.ValidateOAuthToken(oauthRequest.Token);
            if (!isValid)
            {
                var errorResponse = new ErrorResponse("OAuth token inválido.");
                return Unauthorized(errorResponse);
            }

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.UsuarioCorreo == oauthRequest.Email);
            if (usuario == null)
            {
                var errorResponse = new ErrorResponse("Usuario no encontrado.");
                return BadRequest(errorResponse);
            }

            var token = _jwtService.GenerateJwtToken(usuario);
            var successResponse = new SuccessResponse("Login exitoso", new { token });
            return Ok(successResponse);
        }
    }
}
