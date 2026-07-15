using System.Windows.Controls;
using PapeleriaDB.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace PapeleriaDB.Views
{
    public partial class HistorialLogsView : UserControl
    {
        public HistorialLogsView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<HistorialLogsViewModel>();
            Loaded += HistorialLogsView_Loaded;
        }

        private async void HistorialLogsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is HistorialLogsViewModel viewModel) await viewModel.LoadAsync();
        }
    }
}
