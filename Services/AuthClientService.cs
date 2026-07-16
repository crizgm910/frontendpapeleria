using System.Threading.Tasks;
using PapeleriaDB.Models;
using System;
using System.Linq;

namespace PapeleriaDB.Services
{
    public class AuthClientService
    {
        private readonly ApiService _apiService;
        private readonly ApplicationSession _session;

        public AuthClientService(ApiService apiService, ApplicationSession session)
        {
            _apiService = apiService;
            _session = session;
        }

        public async Task<AuthResponseDto> LoginAsync(string username, string password)
        {
            var dto = new LoginDto { Username = username, Password = password };
            
            try
            {
                var response = await _apiService.PostAsync<LoginDto, AuthResponseDto>("api/auth/login", dto);
                
                if (response != null && response.Exito && !string.IsNullOrEmpty(response.Token))
                {
                    _apiService.SetToken(response.Token);
                    _session.Start(response.Token);
                    await AssignCajaPrincipalAsync();
                }
                
                return response ?? new AuthResponseDto { Exito = false, Mensaje = "Error desconocido." };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { Exito = false, Mensaje = ex.Message };
            }
        }

        private async Task AssignCajaPrincipalAsync()
        {
            var cajas = await _apiService.GetAsync<CajaDto[]>("api/cajas");
            var principal = cajas.FirstOrDefault(c =>
                                c.Nombre.Equals("Caja Principal", StringComparison.OrdinalIgnoreCase))
                            ?? cajas.OrderBy(c => c.Id).FirstOrDefault();

            if (principal is not null)
                _session.AssignCaja(principal.Id, _session.TerminalId);
        }

        public void Logout()
        {
            _apiService.SetToken(null);
            _session.End();
        }
    }
}
