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

        public ServiciosViewModel(ApiService apiService)
        {
            _apiService = apiService;
            // La carga ahora se dispara desde el evento Loaded en ServiciosView.xaml.cs
        }

        [RelayCommand]
        public async Task CargarServiciosAsync()
        {
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
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Fallo al cargar servicios: {ex.Message}");
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
