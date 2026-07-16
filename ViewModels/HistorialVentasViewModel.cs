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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace PapeleriaDB.ViewModels
{
    public partial class HistorialVentasViewModel : ObservableRecipient
    {
        private readonly ApiService _apiService;
        private readonly TicketPrintingService _ticketPrintingService;
        private readonly ApplicationSession _session;

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

        public HistorialVentasViewModel(ApiService apiService, TicketPrintingService ticketPrintingService, ApplicationSession session)
        {
            _apiService = apiService;
            _ticketPrintingService = ticketPrintingService;
            _session = session;
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
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Guardar reporte de ventas",
                FileName = $"ventas-{DateTime.Now:yyyyMMdd-HHmm}",
                DefaultExt = ".csv",
                Filter = "Archivo CSV (*.csv)|*.csv"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                static string Csv(string valor) => $"\"{valor.Replace("\"", "\"\"")}\"";
                var contenido = new StringBuilder("Folio,Metodo,Fecha,Estado,Subtotal,Descuento,Total\r\n");
                foreach (var venta in VentasView.Cast<VentaDto>())
                {
                    contenido.AppendLine(string.Join(",",
                        Csv(venta.Folio), Csv(venta.MetodoPago), Csv(venta.Fecha.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)),
                        Csv(venta.Estado), venta.Subtotal.ToString(CultureInfo.InvariantCulture),
                        venta.Descuento.ToString(CultureInfo.InvariantCulture), venta.Total.ToString(CultureInfo.InvariantCulture)));
                }
                File.WriteAllText(dialog.FileName, contenido.ToString(), new UTF8Encoding(true));
                MessageBox.Show($"Reporte guardado correctamente en:\n\n{dialog.FileName}", "Exportación terminada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar el reporte.\n{ex.Message}", "Error al exportar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
