namespace PapeleriaDB.Models
{
    public class HistorialVenta
    {
        public string Folio { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
