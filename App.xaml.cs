using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PapeleriaDB.Services;
using PapeleriaDB.ViewModels;
using System;

namespace PapeleriaDB;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public new static App Current => (App)System.Windows.Application.Current;
    public IServiceProvider Services { get; }

    public App()
    {
        Services = ConfigureServices();
        this.InitializeComponent();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<ApiService>();
        services.AddSingleton<ApplicationSession>();
        services.AddSingleton<AuthClientService>();
        services.AddSingleton<TicketPrintingService>();
        services.AddSingleton<ReportExportService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<PanelResumenViewModel>();
        services.AddTransient<NuevaVentaViewModel>();
        services.AddTransient<HistorialVentasViewModel>();
        services.AddTransient<InventarioViewModel>();
        services.AddTransient<IngresoDeStockViewModel>();
        services.AddTransient<ConfiguracionViewModel>();
        services.AddSingleton<ServiciosViewModel>(); // Registro correcto: Una única instancia compartida globalmente
        services.AddTransient<HistorialLogsViewModel>();
        services.AddTransient<CorteCajaViewModel>();
        services.AddTransient<MovimientosCajaViewModel>();

        return services.BuildServiceProvider();
    }
}
