namespace AuthService.Models
{
    /// <summary>
    /// Representa una solicitud de autenticación OAuth.
    /// Contiene el token recibido desde el proveedor de identidad (como Google, Facebook, etc.)
    /// y el correo electrónico del usuario autenticado.
    /// </summary>
    public class AuthServiceOAuthRequest
    {
        /// <summary>
        /// Obtiene o establece el token de autenticación recibido del proveedor OAuth.
        /// </summary>
        /// <remarks>
        /// Este token se utiliza para verificar la identidad del usuario ante el backend.
        /// Generalmente es un JWT (JSON Web Token) o un token de acceso.
        /// </remarks>
        public string Token { get; set; }

        /// <summary>
        /// Obtiene o establece la dirección de correo electrónico del usuario autenticado.
        /// </summary>
        /// <remarks>
        /// Este campo permite asociar el token con un usuario específico del sistema.
        /// </remarks>
        public string Email { get; set; }
    }
}