using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels;

public partial class IngresoDeStockViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    public ObservableCollection<ProductoDto> ProductosDisponibles { get; } = [];
    public ObservableCollection<IngresoStockArticulo> Articulos { get; } = [];

    [ObservableProperty] private string _folio = $"STK-{DateTime.Now:yyyyMMdd-HHmm}";
    [ObservableProperty] private string _motivo = string.Empty;
    [ObservableProperty] private string _fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    [ObservableProperty] private string _responsable = "Usuario conectado";
    [ObservableProperty] private ProductoDto? _productoSeleccionado;
    [ObservableProperty] private int _cantidadNueva = 1;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string _mensaje = "Selecciona productos y especifica la cantidad recibida.";

    public int TotalArticulos => Articulos.Count;
    public int TotalUnidades => Articulos.Sum(a => Math.Max(0, a.CantidadEntrada));

    public IngresoDeStockViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Articulos.CollectionChanged += Articulos_CollectionChanged;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            var productos = await _apiService.GetAsync<List<ProductoDto>>("api/productos");
            ProductosDisponibles.Clear();
            foreach (var producto in productos.OrderBy(p => p.Nombre)) ProductosDisponibles.Add(producto);
            ProductoSeleccionado ??= ProductosDisponibles.FirstOrDefault();
            Mensaje = productos.Count == 0 ? "No hay productos registrados." : "Productos reales cargados desde el servidor.";
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudieron cargar los productos: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void AgregarProducto()
    {
        if (ProductoSeleccionado is null) { Mensaje = "Selecciona un producto."; return; }
        if (CantidadNueva <= 0) { Mensaje = "La cantidad debe ser mayor que cero."; return; }

        var existente = Articulos.FirstOrDefault(a => a.ProductoId == ProductoSeleccionado.Id);
        if (existente is not null) existente.CantidadEntrada += CantidadNueva;
        else
        {
            Articulos.Add(new IngresoStockArticulo
            {
                ProductoId = ProductoSeleccionado.Id,
                Nombre = ProductoSeleccionado.Nombre,
                Sku = ProductoSeleccionado.CodigoInterno,
                StockActual = ProductoSeleccionado.StockActual,
                CantidadEntrada = CantidadNueva
            });
        }
        CantidadNueva = 1;
        Mensaje = "Producto agregado al ingreso.";
        NotifyTotals();
    }

    [RelayCommand]
    private void QuitarProducto(IngresoStockArticulo? articulo)
    {
        if (articulo is not null) Articulos.Remove(articulo);
    }

    [RelayCommand]
    private async Task GuardarIngresoAsync()
    {
        if (IsSaving) return;
        if (Articulos.Count == 0) { Mensaje = "Agrega al menos un producto."; return; }
        if (string.IsNullOrWhiteSpace(Motivo)) { Mensaje = "Escribe el proveedor o motivo del ingreso."; return; }
        if (Articulos.Any(a => a.CantidadEntrada <= 0)) { Mensaje = "Todas las cantidades deben ser mayores que cero."; return; }

        IsSaving = true;
        var confirmados = 0;
        try
        {
            foreach (var articulo in Articulos.ToList())
            {
                await _apiService.PostAsync<RegistrarMovimientoInventarioDto, MovimientoInventarioDto>(
                    "api/movimientos-inventario",
                    new RegistrarMovimientoInventarioDto
                    {
                        ProductoId = articulo.ProductoId,
                        Tipo = "Entrada",
                        Cantidad = articulo.CantidadEntrada,
                        Motivo = Motivo.Trim()
                    });
                Articulos.Remove(articulo);
                confirmados++;
            }
            Folio = $"STK-{DateTime.Now:yyyyMMdd-HHmm}";
            Fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            Mensaje = $"Ingreso guardado: {confirmados} producto(s) actualizado(s).";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Mensaje = $"Se guardaron {confirmados} producto(s). El resto no se procesó: {ex.Message}";
        }
        finally { IsSaving = false; NotifyTotals(); }
    }

    private void Articulos_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null) foreach (INotifyPropertyChanged item in e.NewItems) item.PropertyChanged += Item_PropertyChanged;
        if (e.OldItems is not null) foreach (INotifyPropertyChanged item in e.OldItems) item.PropertyChanged -= Item_PropertyChanged;
        NotifyTotals();
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) => NotifyTotals();
    private void NotifyTotals() { OnPropertyChanged(nameof(TotalArticulos)); OnPropertyChanged(nameof(TotalUnidades)); }
}
