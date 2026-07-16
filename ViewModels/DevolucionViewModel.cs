using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels;

public partial class DevolucionLineaViewModel : ObservableObject
{
    public int DetalleId { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public int CantidadDisponible { get; init; }
    public decimal PrecioUnitario { get; init; }
    [ObservableProperty] private int _cantidadDevolver;
    public decimal Reembolso => CantidadDevolver * PrecioUnitario;
    partial void OnCantidadDevolverChanged(int value) => OnPropertyChanged(nameof(Reembolso));
}

public partial class DevolucionViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly ApplicationSession _session;
    private readonly VentaDto _venta;

    public ObservableCollection<DevolucionLineaViewModel> Lineas { get; } = [];
    public string Folio => _venta.Folio;

    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private string _mensaje = "Indica cuántas unidades devolver.";

    public DevolucionViewModel(ApiService apiService, ApplicationSession session, VentaDto venta)
    {
        _apiService = apiService;
        _session = session;
        _venta = venta;
        foreach (var detalle in venta.Detalles.Where(d => d.Cantidad > 0))
        {
            var linea = new DevolucionLineaViewModel
            {
                DetalleId = detalle.Id, Descripcion = detalle.Descripcion, Tipo = detalle.Tipo,
                CantidadDisponible = detalle.Cantidad, PrecioUnitario = detalle.PrecioUnitario
            };
            linea.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ReembolsoEstimado));
            Lineas.Add(linea);
        }
    }

    public decimal ReembolsoEstimado => Lineas.Sum(l => l.Reembolso);

    [RelayCommand]
    private void SeleccionarTodo()
    {
        foreach (var linea in Lineas) linea.CantidadDevolver = linea.CantidadDisponible;
        OnPropertyChanged(nameof(ReembolsoEstimado));
    }

    [RelayCommand]
    private async Task ProcesarAsync(Window? window)
    {
        if (IsProcessing) return;
        var seleccion = Lineas.Where(l => l.CantidadDevolver > 0).ToList();
        if (seleccion.Count == 0) { Mensaje = "Selecciona al menos una unidad para devolver."; return; }
        if (seleccion.Any(l => l.CantidadDevolver > l.CantidadDisponible)) { Mensaje = "Una cantidad supera lo vendido."; return; }

        var confirmacion = MessageBox.Show(
            $"Se devolverán {seleccion.Sum(x => x.CantidadDevolver)} unidad(es) por aproximadamente {ReembolsoEstimado:C2}. ¿Continuar?",
            "Confirmar devolución", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirmacion != MessageBoxResult.Yes) return;

        try
        {
            IsProcessing = true;
            var contextoCaja = _session.CajaId > 0
                ? $"?cajaId={_session.CajaId}&usuarioId={_session.UsuarioId}"
                : string.Empty;
            var result = await _apiService.PostAsync<List<DevolucionItemDto>, DevolucionResponseDto>(
                $"api/ventas/{_venta.Id}/devolucion{contextoCaja}",
                seleccion.Select(l => new DevolucionItemDto { DetalleVentaId = l.DetalleId, CantidadDevolver = l.CantidadDevolver }).ToList());
            if (!result.Exito) { Mensaje = result.Mensaje; return; }
            MessageBox.Show($"Devolución completada. Reembolso: {result.MontoReembolsado:C2}", "Devolución", MessageBoxButton.OK, MessageBoxImage.Information);
            if (window is not null) { window.DialogResult = true; window.Close(); }
        }
        catch (Exception ex) { Mensaje = $"No se pudo procesar la devolución: {ex.Message}"; }
        finally { IsProcessing = false; }
    }
}
