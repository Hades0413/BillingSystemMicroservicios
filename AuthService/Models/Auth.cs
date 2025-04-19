using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models;

public class Usuario
{
    [Column("usuario_id")] public int UsuarioId { get; set; }

    [Column("usuario_correo")] public string UsuarioCorreo { get; set; }

    [Column("usuario_contrasena")] public string UsuarioContrasena { get; set; }

    [Column("usuario_telefono")] public string UsuarioTelefono { get; set; }

    [Column("usuario_nombres")] public string UsuarioNombres { get; set; }

    [Column("usuario_apellidos")] public string UsuarioApellidos { get; set; }

    [Column("usuario_fecha_ultima_actualizacion")]
    public DateTime UsuarioFechaUltimaActualizacion { get; set; } = DateTime.Now;
}