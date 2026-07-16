using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels;

public partial class ProductoEditorViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly int? _productoId;

    public ObservableCollection<CategoriaDto> Categorias { get; } = new();
    public bool EsSoloLectura { get; }
    public string Titulo => EsSoloLectura ? "Detalle del producto" : _productoId.HasValue ? "Editar producto" : "Nuevo producto";
    public string TextoBoton => EsSoloLectura ? "Cerrar" : "Guardar producto";

    [ObservableProperty] private string _codigoInterno = string.Empty;
    [ObservableProperty] private string _codigoBarras = string.Empty;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private CategoriaDto? _categoriaSeleccionada;
    [ObservableProperty] private decimal _precioVenta;
    [ObservableProperty] private decimal _costoCompra;
    [ObservableProperty] private int _stockActual;
    [ObservableProperty] private int _stockMinimo = 5;
    [ObservableProperty] private string _descripcion = string.Empty;
    [ObservableProperty] private bool _isProcessing;

    public ProductoEditorViewModel(ApiService apiService, IEnumerable<CategoriaDto> categorias, Articulo? articulo, bool soloLectura)
    {
        _apiService = apiService;
        EsSoloLectura = soloLectura;
        foreach (var categoria in categorias) Categorias.Add(categoria);

        if (articulo is not null)
        {
            _productoId = articulo.Id;
            CodigoInterno = articulo.Sku;
            CodigoBarras = articulo.CodigoBarras;
            Nombre = articulo.Nombre;
            CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Id == articulo.CategoriaId);
            PrecioVenta = articulo.Precio;
            CostoCompra = articulo.CostoCompra;
            StockActual = articulo.Stock;
            StockMinimo = articulo.StockMinimo;
            Descripcion = articulo.Descripcion;
        }
        else
        {
            CategoriaSeleccionada = Categorias.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task GuardarAsync(Window? window)
    {
        if (EsSoloLectura)
        {
            window?.Close();
            return;
        }

        if (string.IsNullOrWhiteSpace(Nombre) || CategoriaSeleccionada is null || PrecioVenta < 0 || CostoCompra < 0 || StockActual < 0 || StockMinimo < 0)
        {
            MessageBox.Show("Completa el nombre, la categoría y usa cantidades válidas.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dto = new GuardarProductoDto
        {
            CodigoInterno = string.IsNullOrWhiteSpace(CodigoInterno) ? $"AUTO-{Guid.NewGuid():N}" : CodigoInterno.Trim(),
            CodigoBarras = string.IsNullOrWhiteSpace(CodigoBarras) ? null : CodigoBarras.Trim(),
            Nombre = Nombre.Trim(),
            CategoriaId = CategoriaSeleccionada.Id,
            PrecioVenta = PrecioVenta,
            CostoCompra = CostoCompra,
            StockActual = StockActual,
            StockMinimo = StockMinimo,
            Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim()
        };

        IsProcessing = true;
        try
        {
            if (_productoId.HasValue)
                await _apiService.PutAsync<GuardarProductoDto, ProductoDto>($"api/productos/{_productoId.Value}", dto);
            else
                await _apiService.PostAsync<GuardarProductoDto, ProductoDto>("api/productos", dto);

            if (window is not null) window.DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo guardar el producto.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
