using System.Threading.Tasks;
using PapeleriaDB.Models;
using System;

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
                }
                
                return response ?? new AuthResponseDto { Exito = false, Mensaje = "Error desconocido." };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { Exito = false, Mensaje = ex.Message };
            }
        }

        public void Logout()
        {
            _apiService.SetToken(null);
            _session.End();
        }
    }
}
