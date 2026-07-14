namespace PapeleriaDB.Models
{
    public class LogItem
    {
        public string Id { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
    }
}
