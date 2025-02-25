namespace BillingService.Models
{
    public class VentaRequest
    {
        public int UsuarioId { get; set; }
        public int EmpresaId { get; set; }
        public int ClienteId { get; set; }
        public int TipoComprobanteId { get; set; }
        public string FormaPago { get; set; }
        public List<VentaProducto> DetallesVenta { get; set; }
        public string ClienteRuc { get; set; }
    }

}