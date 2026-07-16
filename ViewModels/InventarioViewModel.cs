using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels
{
    public partial class InventarioViewModel : ObservableRecipient
    {
        private readonly ApiService _apiService;

        public ObservableCollection<Articulo> Articulos { get; } = new();
        private readonly List<CategoriaDto> _categorias = new();
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
                var productosTask = _apiService.GetAsync<System.Collections.Generic.IEnumerable<ProductoDto>>("api/productos");
                var categoriasTask = _apiService.GetAsync<System.Collections.Generic.IEnumerable<CategoriaDto>>("api/categorias");
                await Task.WhenAll(productosTask, categoriasTask);

                var productos = await productosTask;
                var categorias = (await categoriasTask).ToDictionary(c => c.Id, c => c.Nombre);
                _categorias.Clear();
                _categorias.AddRange(await categoriasTask);
                
                Articulos.Clear();
                foreach (var p in productos)
                {
                    string estado = p.StockActual > 10 ? "Activo" : (p.StockActual > 0 ? "Bajo stock" : "Sin stock");
                    
                    Articulos.Add(new Articulo 
                    { 
                        Id = p.Id,
                        CategoriaId = p.CategoriaId,
                        CodigoBarras = p.CodigoBarras,
                        Descripcion = p.Descripcion ?? string.Empty,
                        Nombre = p.Nombre, 
                        Sku = !string.IsNullOrWhiteSpace(p.CodigoInterno) ? p.CodigoInterno : p.CodigoBarras,
                        Categoria = categorias.TryGetValue(p.CategoriaId, out var categoria) ? categoria : "Sin categoría",
                        Precio = p.PrecioVenta, 
                        CostoCompra = p.CostoCompra,
                        Stock = p.StockActual, 
                        StockMinimo = p.StockMinimo,
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

        [RelayCommand]
        private async Task NuevoArticuloAsync()
        {
            if (_categorias.Count == 0) await LoadDataAsync();
            var modal = new Views.ProductoEditorWindow(new ProductoEditorViewModel(_apiService, _categorias, null, false))
            {
                Owner = Application.Current.MainWindow
            };
            if (modal.ShowDialog() == true) await LoadDataAsync();
        }

        [RelayCommand]
        private async Task VerArticuloAsync(Articulo? articulo)
        {
            if (articulo is null) return;
            var modal = new Views.ProductoEditorWindow(new ProductoEditorViewModel(_apiService, _categorias, articulo, true))
            {
                Owner = Application.Current.MainWindow
            };
            modal.ShowDialog();
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task EditarArticuloAsync(Articulo? articulo)
        {
            if (articulo is null) return;
            var modal = new Views.ProductoEditorWindow(new ProductoEditorViewModel(_apiService, _categorias, articulo, false))
            {
                Owner = Application.Current.MainWindow
            };
            if (modal.ShowDialog() == true) await LoadDataAsync();
        }

        [RelayCommand]
        private async Task ImportarAsync()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar catálogo CSV",
                Filter = "Archivo CSV (*.csv)|*.csv",
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                var lineas = await File.ReadAllLinesAsync(dialog.FileName, Encoding.UTF8);
                if (lineas.Length < 2)
                    throw new InvalidDataException("El archivo no contiene productos.");

                var encabezados = SepararCsv(lineas[0]).Select((nombre, indice) => (nombre: nombre.Trim(), indice))
                    .ToDictionary(x => x.nombre, x => x.indice, StringComparer.OrdinalIgnoreCase);
                int exitos = 0;
                var errores = new List<string>();

                for (var i = 1; i < lineas.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lineas[i])) continue;
                    try
                    {
                        var c = SepararCsv(lineas[i]);
                        string Valor(string nombre) => encabezados.TryGetValue(nombre, out var pos) && pos < c.Count ? c[pos].Trim() : string.Empty;
                        var categoria = _categorias.FirstOrDefault(x => x.Nombre.Equals(Valor("Categoria"), StringComparison.OrdinalIgnoreCase));
                        if (categoria is null) throw new InvalidDataException("categoría inexistente");

                        var dto = new GuardarProductoDto
                        {
                            CodigoInterno = string.IsNullOrWhiteSpace(Valor("CodigoInterno")) ? $"AUTO-{Guid.NewGuid():N}" : Valor("CodigoInterno"),
                            CodigoBarras = string.IsNullOrWhiteSpace(Valor("CodigoBarras")) ? null : Valor("CodigoBarras"),
                            Nombre = Valor("Nombre"),
                            CategoriaId = categoria.Id,
                            PrecioVenta = decimal.Parse(Valor("PrecioVenta"), CultureInfo.InvariantCulture),
                            CostoCompra = decimal.TryParse(Valor("CostoCompra"), NumberStyles.Any, CultureInfo.InvariantCulture, out var costo) ? costo : 0,
                            StockActual = int.TryParse(Valor("StockActual"), out var stock) ? stock : 0,
                            StockMinimo = int.TryParse(Valor("StockMinimo"), out var minimo) ? minimo : 5,
                            Descripcion = Valor("Descripcion")
                        };
                        if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new InvalidDataException("nombre vacío");
                        await _apiService.PostAsync<GuardarProductoDto, ProductoDto>("api/productos", dto);
                        exitos++;
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Fila {i + 1}: {ex.Message}");
                    }
                }

                await LoadDataAsync();
                var detalle = errores.Count == 0 ? string.Empty : $"\n\nNo importadas:\n{string.Join("\n", errores.Take(6))}";
                MessageBox.Show($"Se importaron {exitos} producto(s).{detalle}", "Importación terminada", MessageBoxButton.OK,
                    errores.Count == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo importar el archivo.\n{ex.Message}\n\nColumnas esperadas: Nombre, Categoria, PrecioVenta, CodigoInterno, CodigoBarras, CostoCompra, StockActual, StockMinimo y Descripcion.", "Importar catálogo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static List<string> SepararCsv(string linea)
        {
            var resultado = new List<string>();
            var actual = new StringBuilder();
            var entreComillas = false;
            for (var i = 0; i < linea.Length; i++)
            {
                var caracter = linea[i];
                if (caracter == '"')
                {
                    if (entreComillas && i + 1 < linea.Length && linea[i + 1] == '"') { actual.Append('"'); i++; }
                    else entreComillas = !entreComillas;
                }
                else if (caracter == ',' && !entreComillas) { resultado.Add(actual.ToString()); actual.Clear(); }
                else actual.Append(caracter);
            }
            resultado.Add(actual.ToString());
            return resultado;
        }
    }
}
