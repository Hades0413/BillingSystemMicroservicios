using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using AuthService.Models;

namespace AuthService.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(Usuario usuario)
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