using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views
{
    public partial class HistorialVentasView : UserControl
    {
        public HistorialVentasView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<HistorialVentasViewModel>();
        }
    }
}
