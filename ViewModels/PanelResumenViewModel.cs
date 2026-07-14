using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels
{
    public partial class PanelResumenViewModel : ObservableRecipient
    {
        private readonly ApiService _apiService;

        public ObservableCollection<VentaReciente> VentasRecientes { get; } = new();
        public ObservableCollection<ActividadReciente> Actividades { get; } = new();
        public ObservableCollection<ServicioDelDia> Servicios { get; } = new();

        [ObservableProperty]
        private bool _isLoading;

        public PanelResumenViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                // In the future, call /api/reportes/ventas-por-fecha and /api/reportes/bajo-stock
                // For now the DB is empty so we leave the lists empty.
                await Task.Delay(100); // simulate network latency for empty fetch
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show("Error al cargar resumen: " + ex.Message, "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
