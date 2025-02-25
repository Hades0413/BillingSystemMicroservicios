using AuthService.Models;
using BCrypt.Net;

namespace AuthService.Services
{
    public class AuthenticationService
    {
        private readonly AuthDbContext _context;
        private readonly JwtService _jwtService;

        public AuthenticationService(AuthDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public string Login(string correo, string contrasena)
        {
            var usuario = _context.Usuario.FirstOrDefault(u => u.UsuarioCorreo == correo);
            if (usuario == null)
            {
                throw new UnauthorizedAccessException("El usuario no existe.");
            }

            if (!BCrypt.Net.BCrypt.Verify(contrasena, usuario.UsuarioContrasena))
            {
                throw new UnauthorizedAccessException("La contraseña es incorrecta.");
            }

            return _jwtService.GenerateJwtToken(usuario);
        }
    }
}