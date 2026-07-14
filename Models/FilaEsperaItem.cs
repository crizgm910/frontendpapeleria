using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PapeleriaDB.Models
{
    public partial class FilaEsperaItem : ObservableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;

        [ObservableProperty]
        private int _cantidad;

        [ObservableProperty]
        private decimal _precioUnitario;

        public decimal Total => Cantidad * PrecioUnitario;

        [ObservableProperty]
        private bool _isListo;

        partial void OnCantidadChanged(int value) => OnPropertyChanged(nameof(Total));
        partial void OnPrecioUnitarioChanged(decimal value) => OnPropertyChanged(nameof(Total));
    }
}
