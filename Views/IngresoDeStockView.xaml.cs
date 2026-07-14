using System.Windows.Controls;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views
{
    public partial class IngresoDeStockView : UserControl
    {
        public IngresoDeStockView()
        {
            InitializeComponent();
            DataContext = new IngresoDeStockViewModel();
        }
    }
}
