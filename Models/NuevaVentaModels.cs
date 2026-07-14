using CommunityToolkit.Mvvm.ComponentModel;

namespace PapeleriaDB.Models
{
    public partial class DetalleVentaViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _nombre = string.Empty;

        [ObservableProperty]
        private string _sku = string.Empty;

        [ObservableProperty]
        private decimal _precioUnitario;

        [ObservableProperty]
        private int _cantidad;

        [ObservableProperty]
        private decimal _subtotal;

        [ObservableProperty]
        private bool _isServicio;

        partial void OnPrecioUnitarioChanged(decimal value)
        {
            Subtotal = PrecioUnitario * Cantidad;
        }

        partial void OnCantidadChanged(int value)
        {
            Subtotal = PrecioUnitario * Cantidad;
        }
    }
}
