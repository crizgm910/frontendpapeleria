using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views
{
    public partial class NuevaVentaWindow : Window
    {
        public NuevaVentaWindow()
        {
            InitializeComponent();
            
            // Resolve the ViewModel using DI container since it requires ApiService
            DataContext = App.Current.Services.GetRequiredService<NuevaVentaViewModel>();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
