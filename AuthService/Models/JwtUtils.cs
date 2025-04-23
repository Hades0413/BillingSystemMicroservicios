using System.Security.Cryptography;
using System.Text;

namespace AuthService.Models
{
    /// <summary>
    /// Proporciona utilidades para trabajar con JSON Web Tokens (JWT).
    /// Incluye métodos para codificación Base64 URL, generación de firmas y verificación de firmas.
    /// </summary>
    public class JwtUtils
    {
        /// <summary>
        /// Codifica un array de bytes en una cadena Base64 URL segura.
        /// </summary>
        /// <param name="input">Array de bytes a ser codificado.</param>
        /// <returns>Cadena Base64 URL codificada sin caracteres de relleno (=), reemplazando '+' por '-' y '/' por '_'.</returns>
        /// <remarks>
        /// Este método es utilizado para generar la parte del encabezado y el cuerpo del JWT en formato Base64 URL,
        /// que es una variación del Base64 estándar para evitar caracteres no seguros en URL.
        /// </remarks>
        public static string Base64UrlEncode(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);
            // Eliminar los caracteres de relleno (=)
            base64 = base64.Split('=')[0];
            // Reemplazar los caracteres Base64 estándar con los adecuados para URL
            base64 = base64.Replace('+', '-');
            base64 = base64.Replace('/', '_');
            return base64;
        }

        /// <summary>
        /// Genera la firma de un JWT utilizando el algoritmo HMACSHA256.
        /// </summary>
        /// <param name="header">Encabezado del JWT (Base64 URL codificado).</param>
        /// <param name="payload">Cuerpo del JWT (Base64 URL codificado).</param>
        /// <param name="secret">Secreto compartido utilizado para firmar el JWT.</param>
        /// <returns>Cadena Base64 URL codificada que representa la firma generada.</returns>
        /// <remarks>
        /// Este método utiliza el algoritmo HMACSHA256 para generar una firma segura del JWT, utilizando
        /// un secreto compartido que debe ser conocido solo por el servidor y el cliente.
        /// </remarks>
        public static string GenerateSignature(string header, string payload, string secret)
        {
            // Convertir el secreto en bytes UTF8
            var key = Encoding.UTF8.GetBytes(secret);
            // Concatenar el encabezado y el cuerpo del JWT, separados por un punto
            var data = Encoding.UTF8.GetBytes(header + "." + payload);

            using (var hmac = new HMACSHA256(key))
            {
                // Calcular el hash
                var hash = hmac.ComputeHash(data);
                // Retornar la firma codificada en Base64 URL
                return Base64UrlEncode(hash);
            }
        }

        /// <summary>
        /// Verifica la validez de la firma de un JWT.
        /// </summary>
        /// <param name="token">JWT completo, compuesto por encabezado, cuerpo y firma.</param>
        /// <param name="secret">Secreto compartido utilizado para verificar la firma del JWT.</param>
        /// <returns>True si la firma es válida, de lo contrario false.</returns>
        /// <remarks>
        /// Este método descompone el JWT en sus partes (encabezado, cuerpo, firma) y luego genera
        /// una nueva firma con el secreto proporcionado. Si la firma generada coincide con la firma original,
        /// el JWT es considerado válido.
        /// </remarks>
        public static bool VerifySignature(string token, string secret)
        {
            // Separar el JWT en sus partes: encabezado, cuerpo y firma
            var parts = token.Split('.');
            if (parts.Length != 3) return false; // Si el token no tiene 3 partes, es inválido

            var header = parts[0];
            var payload = parts[1];
            var signature = parts[2];

            // Generar la firma esperada
            var expectedSignature = GenerateSignature(header, payload, secret);

            // Comparar la firma generada con la firma del token
            return expectedSignature == signature;
        }
    }
}
