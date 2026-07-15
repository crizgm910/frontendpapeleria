using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PapeleriaDB.Models;
using PapeleriaDB.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PapeleriaDB.ViewModels
{
    public partial class ConfiguracionViewModel : ObservableRecipient
    {
        private readonly ApiService _apiService;
        private readonly ApplicationSession _session;
        private readonly AuthClientService _authService;

        public ObservableCollection<CajaDto> CajasDisponibles { get; } = new();

        public ConfiguracionViewModel(ApiService apiService, ApplicationSession session, AuthClientService authService)
        {
            _apiService = apiService;
            _session = session;
            _authService = authService;
            _terminalId = session.TerminalId;
        }

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

        [ObservableProperty]
        private CajaDto? _cajaSeleccionada;

        [ObservableProperty]
        private string _terminalId = string.Empty;

        [ObservableProperty]
        private string _estadoConfiguracion = "Selecciona la caja que utilizará esta computadora.";

        [ObservableProperty]
        private bool _cargandoCajas;

        [ObservableProperty]
        private decimal _montoInicial;

        [ObservableProperty]
        private bool _abriendoCaja;

        public bool CajaEstaAbierta => CajaSeleccionada?.EstaAbierta == true;
        public bool PuedeAbrirCaja => !CajaEstaAbierta;

        partial void OnCajaSeleccionadaChanged(CajaDto? value)
        {
            OnPropertyChanged(nameof(CajaEstaAbierta));
            OnPropertyChanged(nameof(PuedeAbrirCaja));
        }

        [RelayCommand]
        public async Task LoadCajasAsync()
        {
            if (CargandoCajas) return;
            try
            {
                CargandoCajas = true;
                EstadoConfiguracion = "Consultando cajas disponibles...";
                var cajas = await _apiService.GetAsync<CajaDto[]>("api/cajas");
                CajasDisponibles.Clear();
                foreach (var caja in cajas.OrderBy(c => c.Id)) CajasDisponibles.Add(caja);

                CajaSeleccionada = CajasDisponibles.FirstOrDefault(c => c.Id == _session.CajaId)
                    ?? CajasDisponibles.FirstOrDefault();
                EstadoConfiguracion = CajasDisponibles.Count == 0
                    ? "Todavía no hay cajas registradas."
                    : _session.CajaId > 0
                        ? $"Esta computadora usa la caja #{_session.CajaId}."
                        : "Selecciona una caja y guarda la configuración.";
            }
            catch (Exception ex)
            {
                EstadoConfiguracion = $"No se pudieron cargar las cajas: {ex.Message}";
            }
            finally
            {
                CargandoCajas = false;
            }
        }

        [RelayCommand]
        private void GuardarConfiguracion()
        {
            if (CajaSeleccionada is null)
            {
                EstadoConfiguracion = "Selecciona una caja antes de guardar.";
                return;
            }

            try
            {
                _session.AssignCaja(CajaSeleccionada.Id, TerminalId);
                EstadoConfiguracion = $"Listo. {CajaSeleccionada.Nombre} quedó asignada a {TerminalId.Trim()}.";
            }
            catch (Exception ex)
            {
                EstadoConfiguracion = ex.Message;
            }
        }

        [RelayCommand]
        private async Task AbrirCajaAsync()
        {
            if (CajaSeleccionada is null)
            {
                EstadoConfiguracion = "Selecciona una caja antes de abrirla.";
                return;
            }

            if (_session.UsuarioId <= 0)
            {
                EstadoConfiguracion = "La sesión no identifica al usuario. Vuelve a iniciar sesión.";
                return;
            }

            if (MontoInicial < 0)
            {
                EstadoConfiguracion = "El efectivo inicial no puede ser negativo.";
                return;
            }

            if (CajaSeleccionada.EstaAbierta)
            {
                EstadoConfiguracion = $"{CajaSeleccionada.Nombre} ya está abierta.";
                return;
            }

            try
            {
                AbriendoCaja = true;
                _session.AssignCaja(CajaSeleccionada.Id, TerminalId);
                var response = await _apiService.PostAsync<AbrirCajaDto, CorteCajaResponseDto>(
                    "api/cajas/abrir",
                    new AbrirCajaDto
                    {
                        CajaId = CajaSeleccionada.Id,
                        UsuarioId = _session.UsuarioId,
                        MontoInicial = MontoInicial
                    });

                EstadoConfiguracion = response.Exito
                    ? $"{CajaSeleccionada.Nombre} está abierta y lista para vender."
                    : response.Mensaje;
                await LoadCajasAsync();
            }
            catch (Exception ex)
            {
                EstadoConfiguracion = $"No se pudo abrir la caja: {ex.Message}";
            }
            finally
            {
                AbriendoCaja = false;
            }
        }

        [RelayCommand]
        private void IrACerrarCaja()
        {
            if (!CajaEstaAbierta)
            {
                EstadoConfiguracion = "La caja seleccionada ya está cerrada.";
                return;
            }

            if (CajaSeleccionada is not null && CajaSeleccionada.Id != _session.CajaId)
                _session.AssignCaja(CajaSeleccionada.Id, TerminalId);

            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage("CorteCaja"));
        }

        [RelayCommand]
        private void CerrarSesion()
        {
            var avisoCaja = CajaEstaAbierta
                ? " La caja permanecerá abierta hasta que realices el corte."
                : string.Empty;
            var respuesta = MessageBox.Show(
                $"¿Quieres cerrar la sesión?{avisoCaja}",
                "Cerrar sesión",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (respuesta != MessageBoxResult.Yes) return;

            _authService.Logout();
            WeakReferenceMessenger.Default.Send(new UserAuthenticationChangedMessage(false));
        }
    }
}
