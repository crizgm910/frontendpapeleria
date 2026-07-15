using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels;

public partial class MovimientosCajaViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly ApplicationSession _session;
    public ObservableCollection<MovimientoCajaDto> Movimientos { get; } = [];
    public IReadOnlyList<string> Tipos { get; } = ["Ingreso", "Egreso"];

    [ObservableProperty] private string _nombreCaja = "Sin caja asignada";
    [ObservableProperty] private bool _cajaAbierta;
    [ObservableProperty] private string _tipoSeleccionado = "Ingreso";
    [ObservableProperty] private string _montoTexto = string.Empty;
    [ObservableProperty] private string _motivo = string.Empty;
    [ObservableProperty] private decimal _ingresosTurno;
    [ObservableProperty] private decimal _egresosTurno;
    [ObservableProperty] private decimal _efectivoEsperado;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string _mensaje = string.Empty;

    public MovimientosCajaViewModel(ApiService apiService, ApplicationSession session)
    {
        _apiService = apiService;
        _session = session;
        _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            if (_session.CajaId <= 0) { Limpiar("Selecciona una caja desde Configuración."); return; }
            var caja = await _apiService.GetAsync<CajaSupervisionDto>($"api/cajas/{_session.CajaId}/estado?historial=1");
            NombreCaja = caja.Nombre;
            CajaAbierta = caja.EstaAbierta;
            IngresosTurno = caja.CorteActual?.IngresosExtra ?? 0;
            EgresosTurno = caja.CorteActual?.Egresos ?? 0;
            EfectivoEsperado = caja.CorteActual?.EfectivoEsperado ?? 0;
            var movimientos = await _apiService.GetAsync<List<MovimientoCajaDto>>($"api/movimientos-caja?cajaId={_session.CajaId}&limit=50");
            Movimientos.Clear();
            foreach (var movimiento in movimientos) Movimientos.Add(movimiento);
            Mensaje = CajaAbierta ? "Caja lista para registrar movimientos." : "La caja está cerrada.";
        }
        catch (Exception ex) { Mensaje = $"No se pudo cargar la caja: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task RegistrarAsync()
    {
        if (IsSaving) return;
        if (!CajaAbierta) { Mensaje = "Abre la caja antes de registrar un ingreso o egreso."; return; }
        if (!TryMoney(MontoTexto, out var monto) || monto <= 0) { Mensaje = "Escribe un monto válido mayor que cero."; return; }
        if (string.IsNullOrWhiteSpace(Motivo)) { Mensaje = "Escribe el motivo del movimiento."; return; }

        var confirmacion = MessageBox.Show($"Registrar {TipoSeleccionado.ToLowerInvariant()} por {monto:C2}: {Motivo.Trim()} ¿Continuar?",
            "Confirmar movimiento", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirmacion != MessageBoxResult.Yes) return;

        try
        {
            IsSaving = true;
            await _apiService.PostAsync<RegistrarMovimientoCajaDto, MovimientoCajaDto>("api/movimientos-caja", new RegistrarMovimientoCajaDto
            {
                CajaId = _session.CajaId, Tipo = TipoSeleccionado, Monto = monto, Motivo = Motivo.Trim()
            });
            MontoTexto = string.Empty;
            Motivo = string.Empty;
            await LoadAsync();
            Mensaje = "Movimiento registrado correctamente.";
        }
        catch (Exception ex) { Mensaje = $"No se pudo registrar el movimiento: {ex.Message}"; }
        finally { IsSaving = false; }
    }

    private void Limpiar(string mensaje)
    {
        NombreCaja = "Sin caja asignada"; CajaAbierta = false; IngresosTurno = 0; EgresosTurno = 0; EfectivoEsperado = 0;
        Movimientos.Clear(); Mensaje = mensaje;
    }

    private static bool TryMoney(string? value, out decimal amount) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out amount) ||
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
}
