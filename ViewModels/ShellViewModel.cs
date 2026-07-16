using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using PapeleriaDB.Messages;
using PapeleriaDB.Models;
using PapeleriaDB.Services;
using System.Collections.ObjectModel;

namespace PapeleriaDB.ViewModels
{
    public partial class ShellViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private object? _currentPage;

        [ObservableProperty]
        private int _totalServicios;

        [ObservableProperty]
        private bool _notificacionesAbiertas;

        [ObservableProperty]
        private bool _cargandoNotificaciones;

        [ObservableProperty]
        private int _totalNotificaciones;

        [ObservableProperty]
        private string _busquedaGlobal = string.Empty;

        public ObservableCollection<string> Notificaciones { get; } = new();
        public bool TieneNotificaciones => TotalNotificaciones > 0;

        private readonly ApiService _apiService;
        private readonly ApplicationSession _session;

        public ShellViewModel(ApiService apiService, ApplicationSession session)
        {
            _apiService = apiService;
            _session = session;
            // Initial view
            CurrentPage = App.Current.Services.GetRequiredService<PanelResumenViewModel>();

            WeakReferenceMessenger.Default.Register<ServiciosActualizadosMessage>(this, (r, m) =>
            {
                TotalServicios = m.TotalCount;
            });

            WeakReferenceMessenger.Default.Register<NavigationRequestedMessage>(this, (r, m) =>
            {
                Navigate(m.Target);
            });
        }

        partial void OnTotalNotificacionesChanged(int value) => OnPropertyChanged(nameof(TieneNotificaciones));

        [RelayCommand]
        private async Task MostrarNotificacionesAsync()
        {
            if (NotificacionesAbiertas)
            {
                NotificacionesAbiertas = false;
                return;
            }

            NotificacionesAbiertas = true;
            CargandoNotificaciones = true;
            Notificaciones.Clear();
            Notificaciones.Add("Consultando avisos...");

            try
            {
                var avisos = new List<string>();
                var dashboard = await _apiService.GetAsync<MobileDashboardDto>("api/mobile/dashboard");
                if (dashboard.TotalAlertasStock > 0)
                    avisos.Add($"{dashboard.TotalAlertasStock} producto(s) necesitan reposición de stock.");

                if (_session.CajaId <= 0)
                {
                    avisos.Add("Esta computadora todavía no tiene una caja asignada.");
                }
                else
                {
                    var caja = await _apiService.GetAsync<CajaSupervisionDto>($"api/cajas/{_session.CajaId}/estado?historial=1");
                    if (!caja.EstaAbierta)
                        avisos.Add($"{caja.Nombre} está cerrada. Ábrela antes de registrar ventas.");
                }

                Notificaciones.Clear();
                foreach (var aviso in avisos) Notificaciones.Add(aviso);
                TotalNotificaciones = avisos.Count;
                if (avisos.Count == 0) Notificaciones.Add("No hay alertas pendientes.");
            }
            catch (Exception)
            {
                Notificaciones.Clear();
                Notificaciones.Add("No se pudieron consultar las alertas. Revisa la conexión e inténtalo otra vez.");
                TotalNotificaciones = 1;
            }
            finally
            {
                CargandoNotificaciones = false;
            }
        }

        [RelayCommand]
        private async Task BuscarGlobalAsync()
        {
            var texto = BusquedaGlobal.Trim();
            if (texto.Length < 2)
            {
                System.Windows.MessageBox.Show("Escribe al menos dos letras para buscar.", "Búsqueda", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            try
            {
                var productosTask = _apiService.GetAsync<List<ProductoDto>>("api/productos");
                var serviciosTask = _apiService.GetAsync<List<ServicioDto>>("api/servicios");
                var ventasTask = _apiService.GetAsync<List<VentaDto>>("api/ventas");
                await Task.WhenAll(productosTask, serviciosTask, ventasTask);

                var resultados = new List<string>();
                resultados.AddRange((await productosTask)
                    .Where(p => p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) || p.CodigoInterno.Contains(texto, StringComparison.OrdinalIgnoreCase) || p.CodigoBarras.Contains(texto, StringComparison.OrdinalIgnoreCase))
                    .Select(p => $"Producto: {p.Nombre} · stock {p.StockActual} · ${p.PrecioVenta:N2}"));
                resultados.AddRange((await serviciosTask)
                    .Where(s => s.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) || s.CodigoInterno.Contains(texto, StringComparison.OrdinalIgnoreCase))
                    .Select(s => $"Servicio: {s.Nombre} · ${s.PrecioBase:N2}"));
                resultados.AddRange((await ventasTask)
                    .Where(v => v.Folio.Contains(texto, StringComparison.OrdinalIgnoreCase) || v.MetodoPago.Contains(texto, StringComparison.OrdinalIgnoreCase))
                    .Select(v => $"Venta: {v.Folio} · {v.Estado} · ${v.Total:N2}"));

                var mensaje = resultados.Count == 0
                    ? "No se encontraron coincidencias."
                    : string.Join("\n", resultados.Take(12)) + (resultados.Count > 12 ? $"\n\nY {resultados.Count - 12} resultado(s) más." : string.Empty);
                System.Windows.MessageBox.Show(mensaje, $"Resultados para “{texto}”", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"No se pudo realizar la búsqueda.\n{ex.Message}", "Búsqueda", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Navigate(string target)
        {
            switch (target)
            {
                case "PanelResumen":
                    CurrentPage = App.Current.Services.GetRequiredService<PanelResumenViewModel>();
                    break;
                case "HistorialVentas":
                    CurrentPage = App.Current.Services.GetRequiredService<HistorialVentasViewModel>();
                    break;
                case "NuevaVenta":
                    var modal = new PapeleriaDB.Views.NuevaVentaWindow();
                    if (App.Current.MainWindow != null)
                    {
                        modal.Owner = App.Current.MainWindow;
                    }
                    modal.ShowDialog();
                    break;
                case "Servicios":
                    CurrentPage = App.Current.Services.GetRequiredService<ServiciosViewModel>(); // Note: Needs to be registered in App.xaml.cs
                    break;
                case "Catalogo":
                    CurrentPage = App.Current.Services.GetRequiredService<InventarioViewModel>();
                    break;
                case "IngresoStock":
                    CurrentPage = App.Current.Services.GetRequiredService<IngresoDeStockViewModel>();
                    break;
                case "HistorialLogs":
                    CurrentPage = App.Current.Services.GetRequiredService<HistorialLogsViewModel>(); // Needs registration
                    break;
                case "Configuracion":
                    CurrentPage = App.Current.Services.GetRequiredService<ConfiguracionViewModel>();
                    break;
                case "CorteCaja":
                    CurrentPage = App.Current.Services.GetRequiredService<CorteCajaViewModel>();
                    break;
                case "MovimientosCaja":
                    CurrentPage = App.Current.Services.GetRequiredService<MovimientosCajaViewModel>();
                    break;
            }
        }
    }
}
