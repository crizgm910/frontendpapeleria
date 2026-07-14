using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using PapeleriaDB.Models;
using Microsoft.Extensions.DependencyInjection;

namespace PapeleriaDB.ViewModels
{
    public partial class MainViewModel : ObservableRecipient, IRecipient<UserAuthenticationChangedMessage>
    {
        [ObservableProperty]
        private object? _currentView;

        public MainViewModel()
        {
            IsActive = true;
            CurrentView = App.Current.Services.GetRequiredService<LoginViewModel>(); // Inicializa con Login para producción
        }

        public void Receive(UserAuthenticationChangedMessage message)
        {
            CurrentView = message.IsAuthenticated ? App.Current.Services.GetRequiredService<ShellViewModel>() : App.Current.Services.GetRequiredService<LoginViewModel>();
        }
    }
}
