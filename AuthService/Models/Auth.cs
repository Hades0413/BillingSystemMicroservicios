using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models
{
    /// <summary>
    /// Representa un modelo de autenticación de usuario en la base de datos.
    /// Esta clase está mapeada a la tabla "Usuario" en la base de datos.
    /// </summary>
    [Table("Usuario")]
    public class Auth
    {
        /// <summary>
        /// Obtiene o establece la dirección de correo electrónico del usuario.
        /// Este campo es la clave primaria de la tabla.
        /// </summary>
        /// <remarks>
        /// - Requerido.
        /// - Debe tener formato de dirección de correo electrónico válida.
        /// </remarks>
        [Key]
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo proporcionado no es una dirección de correo válida.")]
        [Column("usuario_correo")]
        public string Correo { get; set; }

        /// <summary>
        /// Obtiene o establece la contraseña del usuario.
        /// </summary>
        /// <remarks>
        /// - Requerido.
        /// - Mínimo de 8 caracteres.
        /// - Debe almacenarse de forma segura (por ejemplo, con hash y sal).
        /// </remarks>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [Column("usuario_contrasena")]
        public string Contrasena { get; set; }
    }
}