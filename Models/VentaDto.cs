namespace PapeleriaDB.Models
{
    public class VentaDto
    {
        public int Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public System.DateTime Fecha { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public System.Collections.Generic.IEnumerable<VentaHistorialDetalleDto> Detalles { get; set; } = [];
    }

    public class VentaHistorialDetalleDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CrearVentaDto
    {
        public int CajaId { get; set; }
        public int UsuarioId { get; set; }
        public string MetodoPagoPrincipal { get; set; } = "Efectivo";
        public decimal Descuento { get; set; }
        public System.Collections.Generic.List<CrearDetalleVentaDto> Detalles { get; set; } = [];
    }

    public class CrearDetalleVentaDto
    {
        public int? ProductoId { get; set; }
        public int? ServicioId { get; set; }
        public int Cantidad { get; set; }
    }

    public class VentaResponseDto
    {
        public int Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public class DevolucionItemDto
    {
        public int DetalleVentaId { get; set; }
        public int CantidadDevolver { get; set; }
    }

    public class DevolucionResponseDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public decimal MontoReembolsado { get; set; }
    }
}
