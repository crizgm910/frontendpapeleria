namespace PapeleriaDB.Models;

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class ProductoDto
{
    public int Id { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal CostoCompra { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public string? Descripcion { get; set; }
}

public class GuardarProductoDto
{
    public string CodigoInterno { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal CostoCompra { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public string? Descripcion { get; set; }
}

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Estado { get; set; }
}

public class CajaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EstaAbierta { get; set; }
    public string Descripcion => $"{Nombre} - {(EstaAbierta ? "abierta" : "cerrada")}";
}

public class AbrirCajaDto
{
    public int CajaId { get; set; }
    public int UsuarioId { get; set; }
    public decimal MontoInicial { get; set; }
}

public class CorteCajaResponseDto
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public decimal EfectivoEsperado { get; set; }
    public decimal Diferencia { get; set; }
    public decimal TotalVendidoTarjeta { get; set; }
}

public class MobileDashboardDto
{
    public DateTime Fecha { get; set; }
    public ResumenVentasHoyDto VentasHoy { get; set; } = new();
    public int TotalAlertasStock { get; set; }
    public int TotalServiciosActivos { get; set; }
    public int TotalMovimientosHoy { get; set; }
    public List<MovimientoInventarioDto> MovimientosRecientes { get; set; } = [];
}

public class ResumenVentasHoyDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalVentas { get; set; }
    public decimal IngresoTotal { get; set; }
}

public class MovimientoInventarioDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int StockAnterior { get; set; }
    public int StockNuevo { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public class RegistrarMovimientoInventarioDto
{
    public int ProductoId { get; set; }
    public string Tipo { get; set; } = "Entrada";
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public class AuditoriaDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Recurso { get; set; } = string.Empty;
    public string? RecursoId { get; set; }
    public string Metodo { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public int EstadoHttp { get; set; }
    public DateTime Fecha { get; set; }
}
