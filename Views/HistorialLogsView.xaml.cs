using System.Windows.Controls;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views
{
    public partial class HistorialLogsView : UserControl
    {
        public HistorialLogsView()
        {
            InitializeComponent();
            DataContext = new HistorialLogsViewModel();
        }
    }
}
