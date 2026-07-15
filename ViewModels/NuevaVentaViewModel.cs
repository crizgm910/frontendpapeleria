using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PapeleriaDB.ViewModels
{
    public partial class NuevaVentaViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ApplicationSession _session;

        public ObservableCollection<DetalleVentaViewModel> DetallesVenta { get; } = new();

        [ObservableProperty]
        private string _clienteBusqueda = string.Empty;

        [ObservableProperty]
        private string _telefono = string.Empty;

        [ObservableProperty]
        private string _rfc = string.Empty;

        [ObservableProperty]
        private decimal _subtotal;

        [ObservableProperty]
        private decimal _descuento;

        [ObservableProperty]
        private decimal _iva;

        [ObservableProperty]
        private decimal _total;

        [ObservableProperty]
        private string _metodoPagoPrincipal = "Efectivo";

        [ObservableProperty]
        private decimal _montoRecibido;

        [ObservableProperty]
        private bool _isProcessing;

        public NuevaVentaViewModel(ApiService apiService, ApplicationSession session)
        {
            _apiService = apiService;
            _session = session;
            DetallesVenta.CollectionChanged += DetallesVenta_CollectionChanged;
        }

        private void DetallesVenta_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
            RecalcularTotales();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DetalleVentaViewModel.Cantidad) || e.PropertyName == nameof(DetalleVentaViewModel.PrecioUnitario) || e.PropertyName == nameof(DetalleVentaViewModel.Subtotal))
            {
                RecalcularTotales();
            }
        }

        private void RecalcularTotales()
        {
            Subtotal = DetallesVenta.Sum(i => i.Subtotal);
            Descuento = 0m; // Default to 0 for now
            Iva = 0m;
            Total = System.Math.Max(0m, Subtotal - Descuento);
        }

        [RelayCommand]
        private void SetPaymentMethod(string method)
        {
            MetodoPagoPrincipal = method;
        }

        [RelayCommand]
        private async Task RegistrarVentaAsync(Window? window)
        {
            if (DetallesVenta.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_session.UsuarioId <= 0 || _session.CajaId <= 0)
            {
                MessageBox.Show(
                    $"La computadora '{_session.TerminalId}' no tiene una caja asignada. Selecciónala en Configuración.",
                    "Configuración de caja",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            IsProcessing = true;
            try
            {
                var cajas = await _apiService.GetAsync<CajaDto[]>("api/cajas");
                var cajaAsignada = cajas.FirstOrDefault(c => c.Id == _session.CajaId);
                if (cajaAsignada is null)
                {
                    MessageBox.Show("La caja asignada ya no existe. Selecciona otra en Configuración.", "Caja no disponible", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!cajaAsignada.EstaAbierta)
                {
                    MessageBox.Show($"{cajaAsignada.Nombre} está cerrada. Ábrela desde Configuración antes de vender.", "Caja cerrada", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dto = new CrearVentaDto
                {
                    CajaId = _session.CajaId,
                    UsuarioId = _session.UsuarioId,
                    MetodoPagoPrincipal = MetodoPagoPrincipal,
                    Descuento = this.Descuento
                };

                foreach (var item in DetallesVenta)
                {
                    dto.Detalles.Add(new CrearDetalleVentaDto
                    {
                        ProductoId = item.IsServicio ? null : item.Id,
                        ServicioId = item.IsServicio ? item.Id : null,
                        Cantidad = item.Cantidad
                    });
                }

                var response = await _apiService.PostAsync<CrearVentaDto, VentaResponseDto>("api/ventas", dto);
                
                if (response != null && response.Exito)
                {
                    MessageBox.Show("Venta registrada con éxito: " + response.Folio, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (window != null)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Error al registrar venta: " + response?.Mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error de red o servidor: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private void AbrirSelectorArticulo()
        {
            var selector = new PapeleriaDB.Views.SeleccionarObjetoWindow(esServicio: false);
            selector.Owner = App.Current.MainWindow;
            if (selector.ShowDialog() == true)
            {
                var item = ((SeleccionarObjetoViewModel)selector.DataContext).ItemSeleccionado;
                if (item != null)
                {
                    AgregarArticuloALaVenta(item, esServicio: false);
                }
            }
        }

        [RelayCommand]
        private void AbrirSelectorServicio()
        {
            var selector = new PapeleriaDB.Views.SeleccionarObjetoWindow(esServicio: true);
            selector.Owner = App.Current.MainWindow;
            if (selector.ShowDialog() == true)
            {
                var item = ((SeleccionarObjetoViewModel)selector.DataContext).ItemSeleccionado;
                if (item != null)
                {
                    AgregarArticuloALaVenta(item, esServicio: true);
                }
            }
        }

        private void AgregarArticuloALaVenta(ObjetoInventarioDto item, bool esServicio)
        {
            // Check if item is already in list
            var existente = DetallesVenta.FirstOrDefault(d => d.Sku == item.CodigoIdentificador && d.IsServicio == esServicio);
            if (existente != null)
            {
                existente.Cantidad++;
            }
            else
            {
                DetallesVenta.Add(new DetalleVentaViewModel
                {
                    Id = item.Id,
                    Nombre = item.Nombre,
                    Sku = item.CodigoIdentificador,
                    PrecioUnitario = item.PrecioReal,
                    Cantidad = 1,
                    Subtotal = item.PrecioReal,
                    IsServicio = esServicio
                });
            }
        }
    }
}
