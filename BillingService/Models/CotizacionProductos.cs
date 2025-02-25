using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models;

[Table("CotizacionProductos")]
public class CotizacionProducto
{
    [Key]
    [Column("cotizacion_producto_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CotizacionProductoId { get; set; }

    [Column("cotizacion_id")] public int CotizacionId { get; set; }

    [Column("producto_id")] public int ProductoId { get; set; }

    [Column("cantidad")] public int Cantidad { get; set; }

    [Column("precio_unitario")] public decimal PrecioUnitario { get; set; }

    [Column("total")] public decimal Total { get; set; }


    public void CalcularTotal()
    {
        Total = Cantidad * PrecioUnitario;
    }

    public override bool Equals(object obj)
    {
        return obj is CotizacionProducto producto &&
               CotizacionId == producto.CotizacionId &&
               ProductoId == producto.ProductoId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CotizacionId, ProductoId);
    }
}