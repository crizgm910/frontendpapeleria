namespace PapeleriaDB.Models;

public sealed class TicketData
{
    public string Folio { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    public string MetodoPago { get; init; } = string.Empty;
    public string Terminal { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal Descuento { get; init; }
    public decimal Total { get; init; }
    public decimal? MontoRecibido { get; init; }
    public IReadOnlyCollection<TicketLinea> Lineas { get; init; } = [];
}

public sealed class TicketLinea
{
    public string Descripcion { get; init; } = string.Empty;
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal Subtotal { get; init; }
}
