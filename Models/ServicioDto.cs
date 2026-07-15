namespace PapeleriaDB.Models
{
    public class ServicioDto
    {
        public int Id { get; set; }
        public string CodigoInterno { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
    }

    public class GuardarServicioDto
    {
        public string? CodigoInterno { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public string? Descripcion { get; set; }
    }
}
