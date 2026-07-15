namespace PapeleriaDB.Models;

public sealed class CajaSupervisionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EstaAbierta { get; set; }
    public CorteSupervisionDto? CorteActual { get; set; }
    public IReadOnlyList<CorteSupervisionDto> Historial { get; set; } = [];
}

public sealed class CorteSupervisionDto
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal VentasEfectivo { get; set; }
    public decimal VentasTarjeta { get; set; }
    public decimal VentasTransferencia { get; set; }
    public decimal IngresosExtra { get; set; }
    public decimal Egresos { get; set; }
    public decimal EfectivoEsperado { get; set; }
    public decimal? EfectivoContado { get; set; }
    public decimal? Diferencia { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public sealed class CerrarCajaRequest
{
    public int CajaId { get; set; }
    public int UsuarioId { get; set; }
    public decimal EfectivoContado { get; set; }
}
