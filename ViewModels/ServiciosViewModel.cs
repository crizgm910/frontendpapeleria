using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels
{
    public partial class ServiciosViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        public ObservableCollection<ServicioDto> ListaServicios { get; } = new();

        [ObservableProperty]
        private string _estadoActualizacion = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public ServiciosViewModel(ApiService apiService)
        {
            _apiService = apiService;
            // La carga ahora se dispara desde el evento Loaded en ServiciosView.xaml.cs
        }

        [RelayCommand]
        public async Task CargarServiciosAsync()
        {
            IsLoading = true;
            EstadoActualizacion = "Actualizando...";
            try
            {
                // Llamada a la API usando el servicio centralizado que inyecta el Token JWT
                var servicios = await _apiService.GetAsync<System.Collections.Generic.List<ServicioDto>>("api/servicios");

                if (servicios != null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ListaServicios.Clear();
                        foreach (var s in servicios)
                        {
                            ListaServicios.Add(s);
                        }
                    });
                    
                    CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new PapeleriaDB.Messages.ServiciosActualizadosMessage(servicios.Count));
                    EstadoActualizacion = $"Actualizado {System.DateTime.Now:h:mm tt} · {servicios.Count} servicio(s)";
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Fallo al cargar servicios: {ex.Message}");
                EstadoActualizacion = "No se pudo actualizar";
                System.Windows.MessageBox.Show($"No se pudieron actualizar los servicios.\n{ex.Message}", "Servicios", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void AbrirRegistroServicio()
        {
            var modal = new Views.RegistrarServicioWindow();
            modal.Owner = System.Windows.Application.Current.MainWindow;
            if (modal.ShowDialog() == true)
            {
                _ = CargarServiciosAsync();
            }
        }

    }
}
