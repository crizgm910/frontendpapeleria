namespace PapeleriaDB.Models
{
    public class TipoServicioOption
    {
        public string Nombre { get; set; } = string.Empty;
        public string PrecioTexto { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class FilaServicio
    {
        public string IdServicio { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
