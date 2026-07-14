using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PapeleriaDB.Models;
using PapeleriaDB.Services;
using PapeleriaDB.Application.DTOs;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PapeleriaDB.ViewModels
{
    public partial class NuevaVentaViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

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

        public NuevaVentaViewModel(ApiService apiService)
        {
            _apiService = apiService;
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
            Iva = (Subtotal - Descuento) * 0.16m;
            Total = Subtotal - Descuento + Iva;
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

            IsProcessing = true;
            try
            {
                var dto = new CrearVentaDto
                {
                    CajaId = 1,
                    UsuarioId = 1,
                    MetodoPagoPrincipal = MetodoPagoPrincipal,
                    Descuento = this.Descuento,
                    Cliente = string.IsNullOrWhiteSpace(ClienteBusqueda) ? "Público General" : ClienteBusqueda,
                    Telefono = this.Telefono,
                    Subtotal = this.Subtotal,
                    Iva = this.Iva,
                    Total = this.Total,
                    MontoRecibido = this.Total
                };

                foreach (var item in DetallesVenta)
                {
                    dto.Detalles.Add(new CrearDetalleVentaDto
                    {
                        ProductoId = item.IsServicio ? null : item.Id,
                        ServicioId = item.IsServicio ? item.Id : null,
                        Cantidad = item.Cantidad,
                        Sku = item.Sku,
                        Nombre = item.Nombre,
                        Precio = item.PrecioUnitario,
                        Subtotal = item.Subtotal
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
            var existente = DetallesVenta.FirstOrDefault(d => d.Sku == item.Sku);
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
