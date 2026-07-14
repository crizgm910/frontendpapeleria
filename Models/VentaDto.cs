namespace PapeleriaDB.Models
{
    public class VentaDto
    {
        public int Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public System.DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
