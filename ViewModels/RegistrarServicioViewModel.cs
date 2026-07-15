using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels
{
    public partial class RegistrarServicioViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _nombre = string.Empty;

        [ObservableProperty]
        private string _codigoInterno = string.Empty;

        [ObservableProperty]
        private decimal _precio;

        [ObservableProperty]
        private string _descripcion = string.Empty;

        [ObservableProperty]
        private bool _isProcessing;

        public RegistrarServicioViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task GuardarServicioAsync(Window? window)
        {
            if (string.IsNullOrWhiteSpace(Nombre) || Precio <= 0)
            {
                MessageBox.Show("El nombre y el precio unitario son requeridos y deben ser válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsProcessing = true;
            try
            {
                var dto = new GuardarServicioDto
                {
                    Nombre = Nombre,
                    CodigoInterno = string.IsNullOrWhiteSpace(CodigoInterno) ? null : CodigoInterno,
                    PrecioBase = Precio,
                    Descripcion = Descripcion
                };

                var response = await _apiService.PostAsync<GuardarServicioDto, ServicioDto>("api/servicios", dto);
                
                if (response != null)
                {
                    // 2. Resolver el Singleton y forzar la recarga antes de cerrar el modal
                    var vmServicios = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ServiciosViewModel>(App.Current.Services);
                    await vmServicios.CargarServiciosAsync(); // Vuelve a cargar y repoblar la colección desde SQL Server

                    window?.DialogResult = true;
                    window?.Close();
                }
                else
                {
                    MessageBox.Show("Hubo un error al guardar el servicio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error de red o servidor: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
