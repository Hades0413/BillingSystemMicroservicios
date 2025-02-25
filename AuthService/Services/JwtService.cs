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
            // Validar los datos del usuario
            if (usuario == null)
            {
                throw new ArgumentNullException(nameof(usuario), "El usuario no puede ser nulo.");
            }

            // Definir los claims
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.UsuarioCorreo),
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            };

            // Obtener y validar la clave secreta
            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentException("La clave secreta no está configurada en el archivo de configuración.");
            }

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            if (keyBytes.Length < 32)
            {
                throw new ArgumentException("La clave secreta debe tener al menos 32 caracteres (256 bits).");
            }

            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Obtener y validar la duración del token
            if (!int.TryParse(_configuration["Jwt:ExpiryDurationInHours"], out int expiryDurationInHours) ||
                expiryDurationInHours <= 0)
            {
                throw new ArgumentException("La duración de expiración del token es inválida.");
            }

            var expiryDate = DateTime.UtcNow.AddHours(expiryDurationInHours);

            // Crear el token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiryDate,
                signingCredentials: creds
            );

            // Log de token generado
            Console.WriteLine($"Token generado con expiración en: {expiryDate}");

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateJwtToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                // Validar el token
                var validatedToken = tokenHandler.ValidateToken(token, validationParameters, out var _);

                // Log de validación exitosa
                Console.WriteLine("Token validado exitosamente.");

                return true;
            }
            catch (SecurityTokenExpiredException ex)
            {
                // Log de expiración
                Console.WriteLine($"Token expirado: {ex.Message}");
                return false;
            }
            catch (SecurityTokenException ex)
            {
                // Log de token inválido
                Console.WriteLine($"Token de seguridad inválido: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // Log de error general
                Console.WriteLine($"Error general en la validación del token: {ex.Message}");
                return false;
            }
        }
    }
}