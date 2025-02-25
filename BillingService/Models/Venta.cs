using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models;

[Table("Venta")]
public class Venta
{
    [Key] [Column("venta_id")] public int VentaId { get; set; }

    [Column("venta_codigo")] public string VentaCodigo { get; set; } = Guid.NewGuid().ToString();

    [Column("venta_fecha")] public DateTime VentaFecha { get; set; } = DateTime.Now;

    [Column("venta_monto_total")] public decimal VentaMontoTotal { get; set; }

    [Column("venta_monto_descuento")] public decimal VentaMontoDescuento { get; set; }

    [Column("venta_monto_impuesto")] public decimal VentaMontoImpuesto { get; set; }

    [Column("venta_forma_pago")] public string VentaFormaPago { get; set; }

    [Column("tipo_comprobante_id")] public int TipoComprobanteId { get; set; }

    [Column("venta_ruc_cliente")] public string VentaRucCliente { get; set; }

    [Column("usuario_id")] public int UsuarioId { get; set; }

    [Column("empresa_id")] public int EmpresaId { get; set; }

    [Column("cliente_id")] public int ClienteId { get; set; }
}