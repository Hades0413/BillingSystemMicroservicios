using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models;

[Table("TipoComprobante")]
public class TipoComprobante
{
        [Key]
        [Column("tipo_comprobante_id")]
        public int TipoComprobanteId { get; set; }

        [Column("tipo_comprobante_nombre")]
        public string TipoComprobanteNombre { get; set; }

}

