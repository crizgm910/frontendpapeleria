using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PapeleriaDB.Models;
using PapeleriaDB.Services;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views;

public partial class DevolucionWindow : Window
{
    public DevolucionWindow(VentaDto venta)
    {
        InitializeComponent();
        DataContext = new DevolucionViewModel(
            App.Current.Services.GetRequiredService<ApiService>(),
            App.Current.Services.GetRequiredService<ApplicationSession>(),
            venta);
    }
}
