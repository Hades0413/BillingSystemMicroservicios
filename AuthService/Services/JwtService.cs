using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    /// <summary>
    /// Servicio encargado de generar y validar tokens JWT (JSON Web Token) para la autenticación.
    /// </summary>
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="JwtService"/>.
        /// </summary>
        /// <param name="configuration">La configuración de la aplicación para obtener valores como la clave secreta y las configuraciones de expiración del token.</param>
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Genera un token JWT para el usuario autenticado.
        /// </summary>
        /// <param name="auth">Objeto que contiene la información de autenticación del usuario, como el correo.</param>
        /// <returns>El token JWT generado como un string.</returns>
        /// <exception cref="ArgumentNullException">Lanzada si el objeto de autenticación es nulo.</exception>
        /// <exception cref="ArgumentException">Lanzada si la clave secreta o la duración del token no están configuradas correctamente.</exception>
        /// <remarks>
        /// El token generado contiene los siguientes claims:
        /// - <see cref="JwtRegisteredClaimNames.Email"/>: El correo del usuario.
        /// - <see cref="JwtRegisteredClaimNames.Jti"/>: Un identificador único para el token.
        /// - <see cref="JwtRegisteredClaimNames.Iat"/>: La fecha y hora de emisión del token en formato Unix.
        /// </remarks>
        public string GenerateJwtToken(Auth auth)
        {
            if (auth == null) throw new ArgumentNullException(nameof(auth), "El usuario no puede ser nulo.");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, auth.Correo),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Obtener la clave secreta configurada en la configuración de la aplicación.
            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentException("La clave secreta no está configurada en el archivo de configuración.");

            // Convertir la clave secreta en bytes y validarla.
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            if (keyBytes.Length < 32)
                throw new ArgumentException("La clave secreta debe tener al menos 32 caracteres (256 bits).");

            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Validar la duración de expiración del token.
            if (!int.TryParse(_configuration["Jwt:ExpiryDurationInHours"], out var expiryDurationInHours) ||
                expiryDurationInHours <= 0)
                throw new ArgumentException("La duración de expiración del token es inválida.");

            var expiryDate = DateTime.UtcNow.AddHours(expiryDurationInHours);

            // Crear el token JWT.
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiryDate,
                signingCredentials: creds
            );

            Console.WriteLine($"Token generado con expiración en: {expiryDate}");

            // Devolver el token en formato string.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Valida un token JWT recibido.
        /// </summary>
        /// <param name="token">El token JWT que se va a validar.</param>
        /// <returns>Devuelve true si el token es válido, o false si es inválido o ha expirado.</returns>
        /// <remarks>
        /// El token es validado contra la clave secreta, el emisor (Issuer), el público (Audience) y la expiración del token.
        /// </remarks>
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
                    ClockSkew = TimeSpan.Zero // No permitir retraso en la expiración del token.
                };

                // Validar el token.
                var validatedToken = tokenHandler.ValidateToken(token, validationParameters, out _);

                Console.WriteLine("Token validado exitosamente.");

                return true;
            }
            catch (SecurityTokenExpiredException ex)
            {
                Console.WriteLine($"Token expirado: {ex.Message}");
                return false;
            }
            catch (SecurityTokenException ex)
            {
                Console.WriteLine($"Token de seguridad inválido: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general en la validación del token: {ex.Message}");
                return false;
            }
        }
    }
}
