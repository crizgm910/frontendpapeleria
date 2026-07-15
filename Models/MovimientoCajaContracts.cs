namespace PapeleriaDB.Models;

public class RegistrarMovimientoCajaDto
{
    public int CajaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public class MovimientoCajaDto
{
    public int Id { get; set; }
    public int CajaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int UsuarioId { get; set; }
}
