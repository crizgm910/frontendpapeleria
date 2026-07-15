using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels;

public partial class PanelResumenViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly ApplicationSession _session;

    public ObservableCollection<VentaReciente> VentasRecientes { get; } = [];
    public ObservableCollection<ActividadReciente> Actividades { get; } = [];
    public ObservableCollection<ServicioDelDia> Servicios { get; } = [];

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private decimal _ventasDelDia;
    [ObservableProperty] private int _totalVentasHoy;
    [ObservableProperty] private decimal _efectivoEsperado;
    [ObservableProperty] private string _estadoCaja = "Sin caja asignada";
    [ObservableProperty] private int _serviciosHoy;
    [ObservableProperty] private int _alertasStock;
    [ObservableProperty] private string _mensaje = string.Empty;

    public PanelResumenViewModel(ApiService apiService, ApplicationSession session)
    {
        _apiService = apiService;
        _session = session;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        Mensaje = string.Empty;
        try
        {
            var dashboard = await _apiService.GetAsync<MobileDashboardDto>("api/mobile/dashboard");
            var ventas = await _apiService.GetAsync<List<VentaDto>>("api/ventas?limit=30");

            VentasDelDia = dashboard.VentasHoy.IngresoTotal;
            TotalVentasHoy = dashboard.VentasHoy.TotalVentas;
            AlertasStock = dashboard.TotalAlertasStock;

            VentasRecientes.Clear();
            foreach (var venta in ventas.OrderByDescending(v => v.Fecha).Take(6))
            {
                VentasRecientes.Add(new VentaReciente
                {
                    Folio = venta.Folio,
                    Cliente = "Público general",
                    Hora = venta.Fecha.ToLocalTime().ToString("HH:mm"),
                    Estado = venta.Estado,
                    Total = venta.Total
                });
            }

            Actividades.Clear();
            foreach (var movimiento in dashboard.MovimientosRecientes.Take(6))
            {
                Actividades.Add(new ActividadReciente
                {
                    Descripcion = $"{movimiento.ProductoNombre}: {movimiento.Cantidad:+#;-#;0} · {movimiento.Motivo}",
                    Tiempo = movimiento.Fecha.ToLocalTime().ToString("dd/MM HH:mm"),
                    Color = movimiento.Cantidad >= 0 ? "#059669" : "#EF4444"
                });
            }

            Servicios.Clear();
            var servicios = ventas
                .Where(v => v.Fecha.ToLocalTime().Date == DateTime.Now.Date && v.Estado != "Cancelada")
                .SelectMany(v => v.Detalles.Where(d => d.Tipo.Equals("Servicio", StringComparison.OrdinalIgnoreCase))
                    .Select(d => new { Venta = v, Detalle = d }))
                .ToList();
            ServiciosHoy = servicios.Sum(x => x.Detalle.Cantidad);
            foreach (var item in servicios.Take(8))
            {
                Servicios.Add(new ServicioDelDia
                {
                    Folio = item.Venta.Folio,
                    TipoServicio = item.Detalle.Descripcion,
                    Cantidad = item.Detalle.Cantidad,
                    PrecioUnitario = item.Detalle.PrecioUnitario.ToString("C2"),
                    Total = item.Detalle.Subtotal,
                    Hora = item.Venta.Fecha.ToLocalTime().ToString("HH:mm")
                });
            }

            if (_session.CajaId > 0)
            {
                var caja = await _apiService.GetAsync<CajaSupervisionDto>($"api/cajas/{_session.CajaId}/estado?historial=1");
                EfectivoEsperado = caja.CorteActual?.EfectivoEsperado ?? 0;
                EstadoCaja = caja.EstaAbierta ? $"{caja.Nombre} abierta" : $"{caja.Nombre} cerrada";
            }
            else
            {
                EfectivoEsperado = 0;
                EstadoCaja = "Sin caja asignada";
            }
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo actualizar el panel: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
