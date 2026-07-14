using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PapeleriaDB.Services;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views
{
    public partial class SeleccionarObjetoWindow : Window
    {
        public SeleccionarObjetoWindow(bool esServicio)
        {
            InitializeComponent();
            
            // Resolve ApiService from DI container
            var apiService = App.Current.Services.GetRequiredService<ApiService>();
            
            // Assign ViewModel
            DataContext = new SeleccionarObjetoViewModel(apiService, esServicio);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as SeleccionarObjetoViewModel;
            if (vm?.ItemSeleccionado == null)
            {
                MessageBox.Show("Por favor selecciona un elemento de la lista.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            DialogResult = true;
            Close();
        }
    }
}
