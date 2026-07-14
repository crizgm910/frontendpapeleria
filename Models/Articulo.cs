namespace PapeleriaDB.Models
{
    public class Articulo
    {
        public string Nombre { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class CategoriaRapida
    {
        public string Nombre { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
