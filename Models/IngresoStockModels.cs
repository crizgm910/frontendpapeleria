using CommunityToolkit.Mvvm.ComponentModel;

namespace PapeleriaDB.Models
{
    public partial class IngresoStockArticulo : ObservableObject
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int StockActual { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NuevoStock))]
        private int _cantidadEntrada = 1;

        public int NuevoStock => StockActual + CantidadEntrada;
    }
}
