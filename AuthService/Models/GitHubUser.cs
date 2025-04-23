namespace AuthService.Models
{
    /// <summary>
    /// Representa un usuario obtenido desde la API de GitHub.
    /// Contiene la información básica del perfil público del usuario.
    /// </summary>
    public class GitHubUser
    {
        /// <summary>
        /// Obtiene o establece el nombre de usuario (login) de GitHub.
        /// </summary>
        /// <remarks>
        /// Este es el identificador único público del usuario en GitHub.
        /// </remarks>
        public string Login { get; set; }

        /// <summary>
        /// Obtiene o establece la dirección de correo electrónico asociada al usuario de GitHub.
        /// </summary>
        /// <remarks>
        /// Dependiendo de la configuración de privacidad del usuario, este campo puede ser nulo.
        /// </remarks>
        public string Email { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre completo del usuario registrado en GitHub.
        /// </summary>
        /// <remarks>
        /// Puede ser el nombre real del usuario si lo ha proporcionado en su perfil.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Obtiene o establece la URL del avatar del usuario de GitHub.
        /// </summary>
        /// <remarks>
        /// Este campo contiene la dirección de la imagen de perfil del usuario.
        /// </remarks>
        public string AvatarUrl { get; set; }
    }
}