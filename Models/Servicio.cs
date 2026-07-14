using System;

namespace PapeleriaDB.Models
{
    public class Servicio
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioSugerido { get; set; }
        public string Icono { get; set; } = string.Empty;
    }
}
