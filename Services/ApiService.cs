using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace PapeleriaDB.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string? _jwtToken;

        public ApiService()
        {
            var configuredUrl = Environment.GetEnvironmentVariable("PAPELERIA_API_URL")
                ?? "https://papeleria-db-api.onrender.com/";
            if (!configuredUrl.EndsWith('/')) configuredUrl += "/";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuredUrl),
                // Render puede tardar cerca de un minuto en reactivar una instancia gratuita.
                Timeout = TimeSpan.FromSeconds(120)
            };
        }

        public void SetToken(string? token)
        {
            _jwtToken = token;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public string? GetToken() => _jwtToken;

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new JsonException("La API devolvió una respuesta vacía.");
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            
            if (!response.IsSuccessStatusCode)
            {
                // Optionally extract error message
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed: {response.StatusCode}. Content: {errorContent}");
            }
            
            return await response.Content.ReadFromJsonAsync<TResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new JsonException("La API devolvió una respuesta vacía.");
        }
        
        public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new JsonException("La API devolvió una respuesta vacía.");
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
        }
    }
}
