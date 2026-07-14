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
        private string _jwtToken;

        public ApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // Bypass SSL validation for local dev
            };
            
            _httpClient = new HttpClient(handler)
            {
                // The API listens on HTTPS 7001 by default, or HTTP 5000. 
                // We'll set the BaseAddress based on what we usually see in launchSettings.json,
                // but let's default to HTTPS 5001 or 7001. We will find out when we run it.
                // Let's use http://localhost:5000 for now. We can adjust this.
                BaseAddress = new Uri("http://localhost:5139/")
            };
        }

        public void SetToken(string token)
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

        public string GetToken() => _jwtToken;

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            
            return await response.Content.ReadFromJsonAsync<TResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        
        public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
        }
    }
}
