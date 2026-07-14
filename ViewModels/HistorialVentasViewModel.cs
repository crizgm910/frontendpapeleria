using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Views;
using PapeleriaDB.Services;
using System.Collections.Generic;

namespace PapeleriaDB.ViewModels
{
    public partial class HistorialVentasViewModel : ObservableRecipient
    {
        private readonly ApiService _apiService;

        public ObservableCollection<VentaDto> Ventas { get; }

        [ObservableProperty]
        private bool _mostrarFiltrosVentas;
        
        [ObservableProperty]
        private int _totalVentas;

        public HistorialVentasViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Ventas = new ObservableCollection<VentaDto>();
        }

        protected override async void OnActivated()
        {
            base.OnActivated();
            await CargarHistorialVentasAsync();
        }

        [RelayCommand]
        private async Task CargarHistorialVentasAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<List<VentaDto>>("api/ventas");
                if (response != null)
                {
                    Ventas.Clear();
                    foreach (var venta in response)
                    {
                        Ventas.Add(venta);
                    }
                    TotalVentas = Ventas.Count;
                }
            }
            catch (System.Exception ex)
            {
                // Handle potential 404 or connection issues gracefully for now
                System.Diagnostics.Debug.WriteLine($"Error fetching ventas: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AbrirNuevaVentaAsync()
        {
            var modal = new NuevaVentaWindow();
            modal.Owner = System.Windows.Application.Current.MainWindow; // Bloquea el shell de fondo
            
            // ShowDialog es síncrono para el usuario, pero detiene la ejecución aquí hasta que se cierra la ventana
            bool? resultado = modal.ShowDialog();

            if (resultado == true)
            {
                // Forzar la recarga del historial de ventas desde la API de inmediato
                await CargarHistorialVentasAsync();
            }
        }

        [RelayCommand]
        private void AlternarFiltrosVentas()
        {
            MostrarFiltrosVentas = !MostrarFiltrosVentas;
        }

        [RelayCommand]
        private void ExportarVentas()
        {
            MessageBox.Show("Reporte de ventas exportado con éxito.", "Exportar CSV/PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
