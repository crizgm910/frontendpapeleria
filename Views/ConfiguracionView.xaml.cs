using System.Windows.Controls;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views
{
    public partial class ConfiguracionView : UserControl
    {
        public ConfiguracionView()
        {
            InitializeComponent();
            DataContext = new ConfiguracionViewModel();
        }
    }
}
