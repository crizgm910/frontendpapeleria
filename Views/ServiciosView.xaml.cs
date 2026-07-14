using System.Windows.Controls;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views
{
    public partial class ServiciosView : UserControl
    {
        public ServiciosView()
        {
            InitializeComponent();
            // Instancia única y compartida
            this.DataContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ServiciosViewModel>(App.Current.Services);

            // Imprimir el hash de la instancia vinculada a la UI
            System.Diagnostics.Debug.WriteLine($"[DEBUG] ServiciosView vinculada al DataContext Hash: {this.DataContext?.GetHashCode()}");
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ServiciosViewModel vm)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Vista cargada (Event Loaded). Disparando consulta...");
                await vm.CargarServiciosCommand.ExecuteAsync(null);
            }
        }
    }
}
