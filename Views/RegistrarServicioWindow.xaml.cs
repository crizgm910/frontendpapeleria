using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PapeleriaDB.Services;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views
{
    public partial class RegistrarServicioWindow : Window
    {
        public RegistrarServicioWindow()
        {
            InitializeComponent();
            var apiService = App.Current.Services.GetRequiredService<ApiService>();
            DataContext = new RegistrarServicioViewModel(apiService);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
