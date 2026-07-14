using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PapeleriaDB.Models;

namespace PapeleriaDB.ViewModels
{
    public partial class HistorialLogsViewModel : ObservableRecipient
    {
        public ObservableCollection<LogItem> Logs { get; }

        public HistorialLogsViewModel()
        {
            Logs = new ObservableCollection<LogItem>
            {
                new LogItem { Id = "LOG-0541", Usuario = "Cajero 1", Accion = "Venta completada", Detalle = "#VTA-00041", Fecha = "12 may 2024, 10:05" },
                new LogItem { Id = "LOG-0540", Usuario = "Encargado", Accion = "Stock actualizado", Detalle = "Papel Bond +500u", Fecha = "12 may 2024, 09:00" },
                new LogItem { Id = "LOG-0539", Usuario = "Administrador", Accion = "Precio modificado", Detalle = "Folder Manila $3.50", Fecha = "11 may 2024, 18:00" },
                new LogItem { Id = "LOG-0538", Usuario = "Cajero 1", Accion = "Servicio registrado", Detalle = "#SRV-0088 Engargolado", Fecha = "11 may 2024, 16:00" },
                new LogItem { Id = "LOG-0537", Usuario = "Cajero 2", Accion = "Venta completada", Detalle = "#VTA-00039", Fecha = "11 may 2024, 15:30" },
                new LogItem { Id = "LOG-0536", Usuario = "Encargado", Accion = "Alerta stock bajo", Detalle = "Folder Manila (12 u)", Fecha = "11 may 2024, 13:00" }
            };
        }
    }
}
