using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;
using System.Collections.Generic;

namespace PapeleriaDB.ViewModels
{
    public partial class SeleccionarObjetoViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly bool _esServicio;

        public ObservableCollection<ObjetoInventarioDto> ItemsDisponibles { get; set; } = new();

        [ObservableProperty]
        private ObjetoInventarioDto? _itemSeleccionado;

        [ObservableProperty]
        private string _busqueda = string.Empty;

        [ObservableProperty]
        private string _titulo = string.Empty;

        [ObservableProperty]
        private string _descripcion = string.Empty;
        
        [ObservableProperty]
        private bool _mostrarExistencias = true;

        public SeleccionarObjetoViewModel(ApiService apiService, bool esServicio)
        {
            _apiService = apiService;
            _esServicio = esServicio;

            if (_esServicio)
            {
                Titulo = "Seleccionar Servicio";
                Descripcion = "Selecciona un servicio para añadirlo a la venta actual.";
                MostrarExistencias = false;
            }
            else
            {
                Titulo = "Seleccionar Artículo";
                Descripcion = "Selecciona un producto para añadirlo a la venta actual.";
                MostrarExistencias = true;
            }

            _ = CargarItemsAsync();
        }

        private async Task CargarItemsAsync()
        {
            try
            {
                var endpoint = _esServicio ? "api/servicios" : "api/articulos";
                var response = await _apiService.GetAsync<List<ObjetoInventarioDto>>(endpoint);
                
                if (response != null)
                {
                    ItemsDisponibles.Clear();
                    foreach (var item in response)
                    {
                        ItemsDisponibles.Add(item);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching items: {ex.Message}");
            }
        }
    }
}
