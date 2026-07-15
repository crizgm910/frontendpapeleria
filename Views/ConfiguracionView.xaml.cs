using System.Windows.Controls;
using PapeleriaDB.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace PapeleriaDB.Views
{
    public partial class ConfiguracionView : UserControl
    {
        public ConfiguracionView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<ConfiguracionViewModel>();
            Loaded += ConfiguracionView_Loaded;
        }

        private async void ConfiguracionView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ConfiguracionViewModel viewModel)
                await viewModel.LoadCajasAsync();
        }
    }
}
