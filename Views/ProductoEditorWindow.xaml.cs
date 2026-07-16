using System.Windows;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views;

public partial class ProductoEditorWindow : Window
{
    public ProductoEditorWindow(ProductoEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
