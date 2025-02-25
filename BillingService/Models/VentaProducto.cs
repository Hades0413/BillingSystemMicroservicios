using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models
{
    [Table("VentaProductos")]
    public class VentaProducto
    {
        [Key]
        [Column("venta_producto_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Se especifica que es una columna de identidad
        public int VentaProductoId { get; set; } 

        [Column("venta_id")]
        public int VentaId { get; set; }       

        [Column("producto_id")]
        public int ProductoId { get; set; }     

        [Column("cantidad")]
        public int Cantidad { get; set; }      

        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; } 

        [Column("total")]
        public decimal Total { get; set; }    

        public void CalcularTotal()
        {
            Total = Cantidad * PrecioUnitario;
        }

        public override bool Equals(object obj)
        {
            return obj is VentaProducto producto &&
                   VentaId == producto.VentaId &&
                   ProductoId == producto.ProductoId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VentaId, ProductoId);
        }
    }
}