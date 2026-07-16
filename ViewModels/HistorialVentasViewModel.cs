using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Views;
using PapeleriaDB.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace PapeleriaDB.ViewModels
{
    public partial class HistorialVentasViewModel : ObservableRecipient
    {
        private readonly ApiService _apiService;
        private readonly TicketPrintingService _ticketPrintingService;
        private readonly ApplicationSession _session;
        private readonly ReportExportService _reportExportService;

        public ObservableCollection<VentaDto> Ventas { get; }
        public ICollectionView VentasView { get; }
        public IReadOnlyList<string> EstadosDisponibles { get; } = ["Todos", "Completada", "Cancelada"];

        [ObservableProperty]
        private bool _mostrarFiltrosVentas;

        [ObservableProperty]
        private string _textoFiltroVentas = string.Empty;

        [ObservableProperty]
        private string _estadoFiltroVentas = "Todos";
        
        [ObservableProperty]
        private int _totalVentas;

        [ObservableProperty]
        private string _mensaje = string.Empty;

        public HistorialVentasViewModel(ApiService apiService, TicketPrintingService ticketPrintingService, ApplicationSession session, ReportExportService reportExportService)
        {
            _apiService = apiService;
            _ticketPrintingService = ticketPrintingService;
            _session = session;
            _reportExportService = reportExportService;
            Ventas = new ObservableCollection<VentaDto>();
            VentasView = CollectionViewSource.GetDefaultView(Ventas);
            VentasView.Filter = FiltrarVenta;
            _ = CargarHistorialVentasAsync();
        }

        partial void OnTextoFiltroVentasChanged(string value) => ActualizarFiltro();
        partial void OnEstadoFiltroVentasChanged(string value) => ActualizarFiltro();

        private bool FiltrarVenta(object item)
        {
            if (item is not VentaDto venta) return false;
            var coincideEstado = EstadoFiltroVentas == "Todos" || venta.Estado.Equals(EstadoFiltroVentas, StringComparison.OrdinalIgnoreCase);
            var coincideTexto = string.IsNullOrWhiteSpace(TextoFiltroVentas)
                || venta.Folio.Contains(TextoFiltroVentas, StringComparison.OrdinalIgnoreCase)
                || venta.MetodoPago.Contains(TextoFiltroVentas, StringComparison.OrdinalIgnoreCase);
            return coincideEstado && coincideTexto;
        }

        private void ActualizarFiltro()
        {
            VentasView.Refresh();
            TotalVentas = VentasView.Cast<object>().Count();
        }

        protected override async void OnActivated()
        {
            base.OnActivated();
            await CargarHistorialVentasAsync();
        }

        [RelayCommand]
        private async Task CargarHistorialVentasAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<List<VentaDto>>("api/ventas");
                if (response != null)
                {
                    Ventas.Clear();
                    foreach (var venta in response)
                    {
                        Ventas.Add(venta);
                    }
                    ActualizarFiltro();
                }
            }
            catch (System.Exception ex)
            {
                Mensaje = $"No se pudo cargar el historial: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ImprimirTicket(VentaDto? venta)
        {
            if (venta is null) return;
            _ticketPrintingService.Print(new TicketData
            {
                Folio = venta.Folio,
                Fecha = venta.Fecha.ToLocalTime(),
                MetodoPago = venta.MetodoPago,
                Terminal = _session.TerminalId,
                Subtotal = venta.Subtotal,
                Descuento = venta.Descuento,
                Total = venta.Total,
                Lineas = venta.Detalles.Select(item => new TicketLinea
                {
                    Descripcion = item.Descripcion,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = item.Subtotal
                }).ToArray()
            });
        }

        [RelayCommand]
        private async Task AbrirDevolucionAsync(VentaDto? venta)
        {
            if (venta is null) return;
            if (venta.Estado == "Cancelada" || !venta.Detalles.Any(d => d.Cantidad > 0))
            {
                Mensaje = "Esta venta ya no tiene artículos disponibles para devolver.";
                return;
            }

            var modal = new DevolucionWindow(venta) { Owner = System.Windows.Application.Current.MainWindow };
            if (modal.ShowDialog() == true)
            {
                await CargarHistorialVentasAsync();
                Mensaje = "Historial actualizado después de la devolución.";
            }
        }

        [RelayCommand]
        private async Task AbrirNuevaVentaAsync()
        {
            var modal = new NuevaVentaWindow();
            modal.Owner = System.Windows.Application.Current.MainWindow; // Bloquea el shell de fondo
            
            // ShowDialog es síncrono para el usuario, pero detiene la ejecución aquí hasta que se cierra la ventana
            bool? resultado = modal.ShowDialog();

            if (resultado == true)
            {
                // Forzar la recarga del historial de ventas desde la API de inmediato
                await CargarHistorialVentasAsync();
            }
        }

        [RelayCommand]
        private void AlternarFiltrosVentas()
        {
            MostrarFiltrosVentas = !MostrarFiltrosVentas;
        }

        [RelayCommand]
        private void ExportarVentas()
        {
            var ventasExportadas = VentasView.Cast<VentaDto>().ToList();
            if (ventasExportadas.Count == 0)
            {
                MessageBox.Show("No hay ventas visibles para exportar.", "Exportar reporte", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Guardar reporte de ventas",
                FileName = $"ventas-{DateTime.Now:yyyyMMdd-HHmm}",
                DefaultExt = ".xlsx",
                AddExtension = true,
                Filter = "Excel (*.xlsx)|*.xlsx|PDF (*.pdf)|*.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                var descripcionFiltro = EstadoFiltroVentas == "Todos" ? "Todos los estados" : $"Estado: {EstadoFiltroVentas}";
                if (!string.IsNullOrWhiteSpace(TextoFiltroVentas)) descripcionFiltro += $" · Búsqueda: {TextoFiltroVentas.Trim()}";

                if (dialog.FilterIndex == 2)
                    _reportExportService.ExportarPdf(dialog.FileName, ventasExportadas, descripcionFiltro);
                else
                    _reportExportService.ExportarExcel(dialog.FileName, ventasExportadas);

                MessageBox.Show($"Reporte guardado correctamente en:\n\n{dialog.FileName}", "Exportación terminada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar el reporte.\n{ex.Message}", "Error al exportar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
