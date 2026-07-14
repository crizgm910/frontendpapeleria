using System.Windows.Controls;
using PapeleriaDB.ViewModels;

namespace PapeleriaDB.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Input_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (DataContext is LoginViewModel vm && vm.IniciarSesionCommand.CanExecute(PasswordInput))
                {
                    vm.IniciarSesionCommand.Execute(PasswordInput);
                }
            }
        }
    }
}
