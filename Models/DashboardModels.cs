using System;

namespace PapeleriaDB.Models
{
    public class VentaReciente
    {
        public string Folio { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class ActividadReciente
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Tiempo { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty; // Hex color code
    }

    public class ServicioDelDia
    {
        public string Folio { get; set; } = string.Empty;
        public string TipoServicio { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string PrecioUnitario { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Hora { get; set; } = string.Empty;
    }
}
