namespace PapeleriaDB.Models
{
    public class Articulo
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal CostoCompra { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class CategoriaRapida
    {
        public string Nombre { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
