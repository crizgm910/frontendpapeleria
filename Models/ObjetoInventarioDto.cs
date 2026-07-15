namespace PapeleriaDB.Models
{
    public class ObjetoInventarioDto
    {
        public int Id { get; set; }
        public string CodigoInterno { get; set; } = string.Empty;
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int StockActual { get; set; }

        public string CodigoIdentificador => !string.IsNullOrWhiteSpace(CodigoInterno) ? CodigoInterno : CodigoBarras;
        public decimal PrecioReal => Precio;
        public int ExistenciasReales => StockActual;
    }
}
