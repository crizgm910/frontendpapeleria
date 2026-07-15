using System.Windows.Controls;
using PapeleriaDB.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace PapeleriaDB.Views
{
    public partial class IngresoDeStockView : UserControl
    {
        public IngresoDeStockView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<IngresoDeStockViewModel>();
            Loaded += IngresoDeStockView_Loaded;
        }

        private async void IngresoDeStockView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IngresoDeStockViewModel viewModel) await viewModel.LoadAsync();
        }
    }
}
