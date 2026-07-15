using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;
using System.Collections.Generic;
using System.Linq;

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
                ItemsDisponibles.Clear();

                if (_esServicio)
                {
                    var servicios = await _apiService.GetAsync<List<ServicioDto>>("api/servicios");
                    foreach (var servicio in servicios.Where(s => s.Estado))
                    {
                        ItemsDisponibles.Add(new ObjetoInventarioDto
                        {
                            Id = servicio.Id,
                            CodigoInterno = servicio.CodigoInterno,
                            Nombre = servicio.Nombre,
                            Categoria = "Servicio",
                            Precio = servicio.PrecioBase
                        });
                    }
                }
                else
                {
                    var productosTask = _apiService.GetAsync<List<ProductoDto>>("api/productos");
                    var categoriasTask = _apiService.GetAsync<List<CategoriaDto>>("api/categorias");
                    await Task.WhenAll(productosTask, categoriasTask);
                    var categorias = (await categoriasTask).ToDictionary(c => c.Id, c => c.Nombre);

                    foreach (var producto in await productosTask)
                    {
                        ItemsDisponibles.Add(new ObjetoInventarioDto
                        {
                            Id = producto.Id,
                            CodigoInterno = producto.CodigoInterno,
                            CodigoBarras = producto.CodigoBarras,
                            Nombre = producto.Nombre,
                            Categoria = categorias.TryGetValue(producto.CategoriaId, out var categoria) ? categoria : "Sin categoría",
                            Precio = producto.PrecioVenta,
                            StockActual = producto.StockActual
                        });
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
