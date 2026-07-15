using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PapeleriaDB.Models;
using PapeleriaDB.Services;

namespace PapeleriaDB.ViewModels;

public partial class HistorialLogsViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly List<LogItem> _todos = [];
    public ObservableCollection<LogItem> Logs { get; } = [];

    [ObservableProperty] private string _busqueda = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _mensaje = string.Empty;

    public HistorialLogsViewModel(ApiService apiService) => _apiService = apiService;

    partial void OnBusquedaChanged(string value) => AplicarFiltro();

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            var registros = await _apiService.GetAsync<List<AuditoriaDto>>("api/auditoria?limit=200");
            _todos.Clear();
            _todos.AddRange(registros.Select(a => new LogItem
            {
                Id = $"LOG-{a.Id:D5}",
                Usuario = a.UsuarioNombre,
                Accion = a.Accion,
                Recurso = a.Recurso,
                Metodo = a.Metodo,
                Detalle = $"{a.Recurso}{(string.IsNullOrWhiteSpace(a.RecursoId) ? "" : $" #{a.RecursoId}")} · {a.Metodo} · HTTP {a.EstadoHttp}",
                Fecha = a.Fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
            }));
            AplicarFiltro();
            Mensaje = $"{_todos.Count} acciones reales cargadas.";
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo cargar la auditoría: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void Exportar()
    {
        if (Logs.Count == 0) { Mensaje = "No hay registros para exportar."; return; }
        var dialog = new SaveFileDialog { Filter = "Archivo CSV (*.csv)|*.csv", FileName = $"auditoria-{DateTime.Now:yyyyMMdd-HHmm}.csv" };
        if (dialog.ShowDialog() != true) return;

        static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
        var csv = new StringBuilder("ID,Usuario,Accion,Detalle,Fecha\r\n");
        foreach (var item in Logs) csv.AppendLine(string.Join(',', Csv(item.Id), Csv(item.Usuario), Csv(item.Accion), Csv(item.Detalle), Csv(item.Fecha)));
        File.WriteAllText(dialog.FileName, csv.ToString(), new UTF8Encoding(true));
        Mensaje = $"Auditoría exportada: {dialog.FileName}";
    }

    private void AplicarFiltro()
    {
        var texto = Busqueda.Trim();
        var filtrados = string.IsNullOrEmpty(texto) ? _todos : _todos.Where(x =>
            x.Id.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
            x.Usuario.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
            x.Accion.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
            x.Detalle.Contains(texto, StringComparison.OrdinalIgnoreCase));
        Logs.Clear();
        foreach (var item in filtrados) Logs.Add(item);
    }
}
