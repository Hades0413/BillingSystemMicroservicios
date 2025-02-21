using AuthService.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services
{
    public class AuthenticationService
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationService(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            return GenerateJwtToken(usuario);
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.UsuarioCorreo),
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            };

            var secretKey = _configuration["Jwt:SecretKey"];
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            if (keyBytes.Length < 32)
            {
                throw new ArgumentException("La clave secreta debe tener al menos 32 caracteres (256 bits).");
            }

            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(int.Parse(_configuration["Jwt:ExpiryDurationInHours"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
