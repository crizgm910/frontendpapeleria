using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using PapeleriaDB.Messages;

namespace PapeleriaDB.ViewModels
{
    public partial class ShellViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private object? _currentPage;

        [ObservableProperty]
        private int _totalServicios;

        public ShellViewModel()
        {
            // Initial view
            CurrentPage = App.Current.Services.GetRequiredService<PanelResumenViewModel>();

            WeakReferenceMessenger.Default.Register<ServiciosActualizadosMessage>(this, (r, m) =>
            {
                TotalServicios = m.TotalCount;
            });
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
            }
        }
    }
}
