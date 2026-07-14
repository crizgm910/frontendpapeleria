using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PapeleriaDB.Models;

namespace PapeleriaDB.ViewModels
{
    public partial class LoginViewModel : ObservableRecipient
    {
        private readonly Services.AuthClientService _authService;

        [ObservableProperty]
        private string _usuario = string.Empty;

        [ObservableProperty]
        private bool _recordarDispositivo;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel(Services.AuthClientService authService)
        {
            _authService = authService;
            CargarUsuarioRecordado();
        }

        private void CargarUsuarioRecordado()
        {
            try
            {
                if (Properties.Settings.Default.RecordarUsuario)
                {
                    Usuario = Properties.Settings.Default.UsuarioGuardado;
                    RecordarDispositivo = true;
                }
            }
            catch (Exception)
            {
            }
        }

        private void GuardarCredencialesLocales()
        {
            try
            {
                if (RecordarDispositivo)
                {
                    Properties.Settings.Default.UsuarioGuardado = Usuario;
                    Properties.Settings.Default.RecordarUsuario = true;
                }
                else
                {
                    Properties.Settings.Default.UsuarioGuardado = string.Empty;
                    Properties.Settings.Default.RecordarUsuario = false;
                }

                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
            }
        }

        [RelayCommand]
        private async Task IniciarSesionAsync(object? parameter)
        {
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            string password = passwordBox?.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Usuario) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Por favor ingrese su usuario y contraseña.";
                return;
            }
            
            ErrorMessage = string.Empty;
            IsLoading = true;
            
            var response = await _authService.LoginAsync(Usuario, password);
            
            IsLoading = false;

            if (response.Exito)
            {
                GuardarCredencialesLocales();
                WeakReferenceMessenger.Default.Send(new UserAuthenticationChangedMessage(true));
            }
            else
            {
                ErrorMessage = response.Mensaje ?? "Error de inicio de sesión.";
            }
        }
    }
}
