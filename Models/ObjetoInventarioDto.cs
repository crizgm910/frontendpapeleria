namespace PapeleriaDB.Models
{
    public class ObjetoInventarioDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        
        // Mapped from API
        public decimal PrecioVenta { get; set; } // From articulos
        public decimal Precio { get; set; } // From servicios
        public int StockActual { get; set; }
        public int Existencias { get; set; }

        // Unified getters for the frontend
        public string CodigoIdentificador => !string.IsNullOrEmpty(Sku) ? Sku : CodigoBarras;
        public decimal PrecioReal => Precio > 0 ? Precio : PrecioVenta;
        public int ExistenciasReales => StockActual > 0 ? StockActual : Existencias;
    }
}
