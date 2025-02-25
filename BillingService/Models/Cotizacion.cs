using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models;

[Table("Cotizacion")]
public class Cotizacion
{
    [Key] [Column("cotizacion_id")] public int CotizacionId { get; set; }

    [Column("cotizacion_codigo")] public string CotizacionCodigo { get; set; } = Guid.NewGuid().ToString();

    [Column("cotizacion_fecha")] public DateTime CotizacionFecha { get; set; } = DateTime.Now;

    [Column("cotizacion_monto_total")] public decimal CotizacionMontoTotal { get; set; }

    [Column("cotizacion_monto_descuento")] public decimal CotizacionMontoDescuento { get; set; }

    [Column("cotizacion_monto_impuesto")] public decimal CotizacionMontoImpuesto { get; set; }

    [Column("usuario_id")] public int UsuarioId { get; set; }

    [Column("empresa_id")] public int EmpresaId { get; set; }

    [Column("cliente_id")] public int ClienteId { get; set; }
}