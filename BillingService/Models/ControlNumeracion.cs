using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models;

[Table("ControlNumeracion")]
public class ControlNumeracion
{
    [Key]
    [Column("control_numeracion_id")]
    public int ControlNumeracionId { get; set; }

    [Column("tipo_comprobante_id")] public int TipoComprobanteId { get; set; }

    [Column("prefijo")] public string Prefijo { get; set; }

    [Column("numeracion")] public int Numeracion { get; set; }

    [Column("fecha_actualizacion")] public DateTime FechaActualizacion { get; set; } = DateTime.Now;
}