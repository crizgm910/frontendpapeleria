using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;
using PapeleriaDB.Application.DTOs;

namespace PapeleriaDB.ViewModels
{
    public partial class InventarioViewModel : ObservableRecipient
    {
        private readonly ApiService _apiService;

        public ObservableCollection<Articulo> Articulos { get; } = new();
        public ICollectionView ArticulosView { get; }
        
        [ObservableProperty]
        private string _textoBusqueda = string.Empty;

        [ObservableProperty]
        private string _categoriaSeleccionada = "Todos";

        [ObservableProperty]
        private bool _mostrarFiltrosAvanzados;

        [ObservableProperty]
        private bool _isLoading;

        public InventarioViewModel(ApiService apiService)
        {
            _apiService = apiService;
            
            ArticulosView = CollectionViewSource.GetDefaultView(Articulos);
            ArticulosView.Filter = FiltrarArticulos;

            LoadDataCommand.Execute(null);
        }

        partial void OnTextoBusquedaChanged(string value) => ArticulosView.Refresh();
        partial void OnCategoriaSeleccionadaChanged(string value) => ArticulosView.Refresh();

        private bool FiltrarArticulos(object item)
        {
            if (item is not Articulo articulo) return false;

            // Filtro por categoría rápida
            bool cumpleCategoria = CategoriaSeleccionada == "Todos" || 
                                   string.Equals(articulo.Categoria, CategoriaSeleccionada, StringComparison.OrdinalIgnoreCase);

            // Filtro por cuadro de texto (Nombre o SKU)
            bool cumpleBusqueda = string.IsNullOrWhiteSpace(TextoBusqueda) ||
                                  (!string.IsNullOrEmpty(articulo.Nombre) && articulo.Nombre.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                                  (!string.IsNullOrEmpty(articulo.Sku) && articulo.Sku.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase));

            return cumpleCategoria && cumpleBusqueda;
        }

        [RelayCommand]
        private void SeleccionarCategoria(string categoria)
        {
            // Si hace clic en la categoría activa, remueve el filtro
            CategoriaSeleccionada = CategoriaSeleccionada == categoria ? "Todos" : categoria;
        }

        [RelayCommand]
        private void AlternarFiltros()
        {
            MostrarFiltrosAvanzados = !MostrarFiltrosAvanzados;
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var productos = await _apiService.GetAsync<System.Collections.Generic.IEnumerable<ProductoDto>>("api/productos");
                
                Articulos.Clear();
                foreach (var p in productos)
                {
                    string estado = p.StockActual > 10 ? "Activo" : (p.StockActual > 0 ? "Bajo stock" : "Sin stock");
                    
                    // We assign a pseudo category just for testing the filters since the API doesn't return one yet.
                    string pseudoCategoria = "Papelería";
                    if (p.Nombre.Contains("Silla", StringComparison.OrdinalIgnoreCase) || p.Nombre.Contains("Escritorio", StringComparison.OrdinalIgnoreCase)) pseudoCategoria = "Oficina";
                    else if (p.Nombre.Contains("Regalo", StringComparison.OrdinalIgnoreCase) || p.Nombre.Contains("Taza", StringComparison.OrdinalIgnoreCase)) pseudoCategoria = "Regalos";
                    else if (p.Nombre.Contains("Impresión", StringComparison.OrdinalIgnoreCase) || p.Nombre.Contains("Copia", StringComparison.OrdinalIgnoreCase)) pseudoCategoria = "Servicios";

                    Articulos.Add(new Articulo 
                    { 
                        Nombre = p.Nombre, 
                        Sku = p.CodigoBarras, 
                        Categoria = pseudoCategoria, 
                        Precio = p.PrecioVenta, 
                        Stock = p.StockActual, 
                        Estado = estado 
                    });
                }
                ArticulosView.Refresh();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading products: " + ex.Message, "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
