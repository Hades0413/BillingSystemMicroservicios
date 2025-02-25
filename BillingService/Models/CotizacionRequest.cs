namespace BillingService.Models;

public class CotizacionRequest
{
    public int UsuarioId { get; set; }
    public int EmpresaId { get; set; }
    public int ClienteId { get; set; }
    public DateTime CotizacionFecha { get; set; }
    public decimal CotizacionMontoTotal { get; set; }
    public decimal CotizacionMontoDescuento { get; set; }
    public decimal CotizacionMontoImpuesto { get; set; }
    public List<CotizacionProducto> Productos { get; set; }
}