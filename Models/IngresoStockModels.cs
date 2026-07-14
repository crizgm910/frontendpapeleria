namespace PapeleriaDB.Models
{
    public class IngresoStockArticulo
    {
        public string Nombre { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int CantidadEntrada { get; set; }
        public int NuevoStock => StockActual + CantidadEntrada;
    }
}
