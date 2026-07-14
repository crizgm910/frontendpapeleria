using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PapeleriaDB.Models;

namespace PapeleriaDB.ViewModels
{
    public partial class IngresoDeStockViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _folio = "STK-000326";

        [ObservableProperty]
        private string _proveedor = "Seleccionar proveedor";

        [ObservableProperty]
        private string _fecha = "12/05/2024";

        [ObservableProperty]
        private string _responsable = "Encargado";

        public ObservableCollection<IngresoStockArticulo> Articulos { get; }

        public IngresoDeStockViewModel()
        {
            Articulos = new ObservableCollection<IngresoStockArticulo>
            {
                new IngresoStockArticulo { Nombre = "Resma Papel Bond A4", Sku = "PAP-001-A4", StockActual = 200, CantidadEntrada = 500 },
                new IngresoStockArticulo { Nombre = "Folder Manila Carta", Sku = "PAP-002-FM", StockActual = 12, CantidadEntrada = 100 },
                new IngresoStockArticulo { Nombre = "Pluma Ballpoint Azul x12", Sku = "PAP-003-PB", StockActual = 0, CantidadEntrada = 60 },
                new IngresoStockArticulo { Nombre = "Marcador Permanente Negro", Sku = "PAP-005-MP", StockActual = 8, CantidadEntrada = 50 }
            };
        }
    }
}
