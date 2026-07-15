using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels;

public partial class CorteCajaViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly ApplicationSession _session;

    public ObservableCollection<CorteSupervisionDto> Historial { get; } = [];

    [ObservableProperty] private string _nombreCaja = "Caja sin configurar";
    [ObservableProperty] private bool _estaAbierta;
    [ObservableProperty] private decimal _efectivoEsperado;
    [ObservableProperty] private decimal _ventasEfectivo;
    [ObservableProperty] private decimal _ventasTarjeta;
    [ObservableProperty] private decimal _ventasTransferencia;
    [ObservableProperty] private decimal _ingresosExtra;
    [ObservableProperty] private decimal _egresos;
    [ObservableProperty] private string _efectivoContadoTexto = string.Empty;
    [ObservableProperty] private decimal _diferenciaPrevia;
    [ObservableProperty] private bool _confirmarCorte;
    [ObservableProperty] private bool _cargando;
    [ObservableProperty] private bool _cerrando;
    [ObservableProperty] private string _mensaje = string.Empty;
    [ObservableProperty] private bool _resultadoVisible;
    [ObservableProperty] private decimal _resultadoEsperado;
    [ObservableProperty] private decimal _resultadoDiferencia;
    [ObservableProperty] private decimal _resultadoTarjeta;

    public bool PuedeCerrar => EstaAbierta && !Cargando && !Cerrando;
    public string EstadoCaja => EstaAbierta ? "Turno abierto" : "Caja cerrada";
    public string DiferenciaDescripcion => DiferenciaPrevia switch
    {
        > 0 => "Sobrante estimado",
        < 0 => "Faltante estimado",
        _ => "Caja cuadrada"
    };

    public CorteCajaViewModel(ApiService apiService, ApplicationSession session)
    {
        _apiService = apiService;
        _session = session;
        _ = CargarAsync();
    }

    partial void OnEstaAbiertaChanged(bool value)
    {
        OnPropertyChanged(nameof(PuedeCerrar));
        OnPropertyChanged(nameof(EstadoCaja));
    }

    partial void OnCargandoChanged(bool value) => OnPropertyChanged(nameof(PuedeCerrar));
    partial void OnCerrandoChanged(bool value) => OnPropertyChanged(nameof(PuedeCerrar));

    partial void OnEfectivoContadoTextoChanged(string value)
    {
        DiferenciaPrevia = TryParseMoney(value, out var contado) ? contado - EfectivoEsperado : 0;
        OnPropertyChanged(nameof(DiferenciaDescripcion));
    }

    partial void OnDiferenciaPreviaChanged(decimal value) => OnPropertyChanged(nameof(DiferenciaDescripcion));

    [RelayCommand]
    private async Task CargarAsync()
    {
        if (Cargando) return;
        Cargando = true;
        Mensaje = string.Empty;
        try
        {
            if (_session.CajaId <= 0)
            {
                Limpiar("Selecciona una caja para esta computadora desde Configuración.");
                return;
            }

            var caja = await _apiService.GetAsync<CajaSupervisionDto>($"api/cajas/{_session.CajaId}/estado?historial=20");

            NombreCaja = caja.Nombre;
            EstaAbierta = caja.EstaAbierta;
            var corte = caja.CorteActual;
            EfectivoEsperado = corte?.EfectivoEsperado ?? 0;
            VentasEfectivo = corte?.VentasEfectivo ?? 0;
            VentasTarjeta = corte?.VentasTarjeta ?? 0;
            VentasTransferencia = corte?.VentasTransferencia ?? 0;
            IngresosExtra = corte?.IngresosExtra ?? 0;
            Egresos = corte?.Egresos ?? 0;
            Historial.Clear();
            foreach (var item in caja.Historial.OrderByDescending(item => item.FechaCierre ?? item.FechaApertura))
                Historial.Add(item);

            if (!EstaAbierta) Mensaje = "Esta caja no tiene un turno abierto.";
            ActualizarDiferencia();
        }
        catch (Exception ex)
        {
            Mensaje = MensajeAmigable(ex, "No se pudo consultar el estado de la caja.");
        }
        finally
        {
            Cargando = false;
        }
    }

    [RelayCommand]
    private async Task CerrarAsync()
    {
        if (!PuedeCerrar || Cerrando) return;
        Mensaje = string.Empty;
        ResultadoVisible = false;

        if (_session.UsuarioId <= 0)
        {
            Mensaje = "La sesión no contiene un usuario válido. Cierra sesión e ingresa de nuevo.";
            return;
        }
        if (!TryParseMoney(EfectivoContadoTexto, out var contado) || contado < 0)
        {
            Mensaje = "Escribe una cantidad válida de efectivo contado.";
            return;
        }
        if (!ConfirmarCorte)
        {
            Mensaje = "Confirma que terminaste de contar el efectivo antes de cerrar.";
            return;
        }

        Cerrando = true;
        try
        {
            var result = await _apiService.PostAsync<CerrarCajaRequest, CorteCajaResponseDto>(
                "api/cajas/cerrar",
                new CerrarCajaRequest { CajaId = _session.CajaId, UsuarioId = _session.UsuarioId, EfectivoContado = contado });

            ResultadoEsperado = result.EfectivoEsperado;
            ResultadoDiferencia = result.Diferencia;
            ResultadoTarjeta = result.TotalVendidoTarjeta;
            ResultadoVisible = true;
            Mensaje = result.Mensaje;
            EfectivoContadoTexto = string.Empty;
            ConfirmarCorte = false;
            await CargarAsync();
        }
        catch (Exception ex)
        {
            Mensaje = MensajeAmigable(ex, "No se pudo completar el corte. La caja permanece abierta.");
        }
        finally
        {
            Cerrando = false;
        }
    }

    private void ActualizarDiferencia()
    {
        DiferenciaPrevia = TryParseMoney(EfectivoContadoTexto, out var contado) ? contado - EfectivoEsperado : 0;
    }

    private void Limpiar(string mensaje)
    {
        NombreCaja = "Caja sin configurar";
        EstaAbierta = false;
        EfectivoEsperado = 0;
        VentasEfectivo = 0;
        VentasTarjeta = 0;
        VentasTransferencia = 0;
        IngresosExtra = 0;
        Egresos = 0;
        Historial.Clear();
        Mensaje = mensaje;
    }

    private static bool TryParseMoney(string? value, out decimal amount) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out amount) ||
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);

    private static string MensajeAmigable(Exception exception, string fallback)
    {
        var message = exception.Message;
        if (message.Contains("ya no está abierta", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("no está abierta", StringComparison.OrdinalIgnoreCase))
            return "La caja ya fue cerrada desde otra computadora. Actualiza para ver el estado actual.";
        if (message.Contains("No se encontró un turno", StringComparison.OrdinalIgnoreCase))
            return "No existe un turno abierto para cerrar en esta caja.";
        return fallback;
    }
}
