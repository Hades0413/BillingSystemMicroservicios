using AuthService.Data;

namespace AuthService.Services
{
    /// <summary>
    /// Servicio encargado de gestionar la autenticación de usuarios.
    /// Proporciona métodos para iniciar sesión y generar tokens JWT para usuarios autenticados.
    /// </summary>
    public class AuthenticationService
    {
        private readonly AuthDbContext _context;
        private readonly JwtService _jwtService;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AuthenticationService"/>.
        /// </summary>
        /// <param name="context">Instancia del contexto de base de datos <see cref="AuthDbContext"/> que se usa para acceder a los datos de autenticación.</param>
        /// <param name="jwtService">Instancia del servicio <see cref="JwtService"/> que se usa para generar tokens JWT.</param>
        public AuthenticationService(AuthDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Inicia sesión de un usuario verificando su correo electrónico y contraseña.
        /// Si las credenciales son correctas, genera y devuelve un token JWT.
        /// </summary>
        /// <param name="correo">El correo electrónico del usuario que intenta iniciar sesión.</param>
        /// <param name="contrasena">La contraseña del usuario proporcionada en el intento de inicio de sesión.</param>
        /// <returns>Un token JWT que representa la sesión activa del usuario.</returns>
        /// <exception cref="UnauthorizedAccessException">Lanzada cuando el correo no existe o la contraseña es incorrecta.</exception>
        /// <remarks>
        /// Este método primero verifica si el correo existe en la base de datos. Luego, compara la contraseña proporcionada
        /// con la almacenada en la base de datos usando un algoritmo de hash seguro (BCrypt).
        /// Si las credenciales son correctas, se genera un token JWT que se devuelve como resultado.
        /// </remarks>
        public string Login(string correo, string contrasena)
        {
            // Buscar el usuario por su correo electrónico en la base de datos.
            var auth = _context.Auth.FirstOrDefault(auth => auth.Correo == correo);

            // Si no se encuentra el usuario, se lanza una excepción de acceso no autorizado.
            if (auth == null) throw new UnauthorizedAccessException("El usuario no existe.");

            // Verificar si la contraseña proporcionada coincide con la almacenada (usando BCrypt para verificar el hash).
            if (!BCrypt.Net.BCrypt.Verify(contrasena, auth.Contrasena))
                throw new UnauthorizedAccessException("La contraseña es incorrecta.");

            // Generar un token JWT para el usuario autenticado.
            return _jwtService.GenerateJwtToken(auth);
        }
    }
}
