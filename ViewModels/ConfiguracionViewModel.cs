using CommunityToolkit.Mvvm.ComponentModel;

namespace PapeleriaDB.ViewModels
{
    public partial class ConfiguracionViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _nombreUsuario = "Cajero 1";

        [ObservableProperty]
        private string _emailUsuario = "cajero1@papeleriadb.local";

        [ObservableProperty]
        private string _rolUsuario = "Cajero";

        [ObservableProperty]
        private string _idOperador = "OPR-2026-001 (No modificable)";

        [ObservableProperty]
        private string _pinTurno = "••••";
    }
}
