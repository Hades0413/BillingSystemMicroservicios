using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models
{
    public class ControlNumeracion
    {
        [Key]
        [Column("control_numeracion_id")]
        public int ControlNumeracionId { get; set; }

        [Column("tipo_comprobante")]
        public string TipoComprobante { get; set; }

        [Column("prefijo")]
        public string Prefijo { get; set; }

        [Column("numeracion")]
        public int Numeracion { get; set; }
    }
}