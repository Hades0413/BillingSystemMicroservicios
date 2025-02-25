namespace BillingService.Models;

public class ResultadoCotizacion
{
    public bool Success { get; set; }
    public int CotizacionId { get; set; }
    public string Mensaje { get; set; }
}