using System;
using System.Text.Json;
using PapeleriaDB.Properties;

namespace PapeleriaDB.Services;

public sealed class ApplicationSession
{
    public int UsuarioId { get; private set; }
    public int CajaId { get; private set; } = ReadCajaId();
    public string TerminalId { get; private set; } = ReadTerminalId();

    public void Start(string jwtToken)
    {
        UsuarioId = ReadUserId(jwtToken);
    }

    public void End()
    {
        UsuarioId = 0;
    }

    public void AssignCaja(int cajaId, string terminalId)
    {
        if (cajaId <= 0) throw new ArgumentOutOfRangeException(nameof(cajaId));
        if (string.IsNullOrWhiteSpace(terminalId)) throw new ArgumentException("Escribe un nombre para esta computadora.", nameof(terminalId));

        CajaId = cajaId;
        TerminalId = terminalId.Trim();
        Settings.Default.CajaId = CajaId;
        Settings.Default.TerminalId = TerminalId;
        Settings.Default.Save();
    }

    private static int ReadCajaId()
    {
        var configured = Environment.GetEnvironmentVariable("PAPELERIA_CAJA_ID");
        // No existe un límite fijo de cajas. Cada computadora debe recibir
        // cualquier identificador positivo creado previamente en el backend.
        if (int.TryParse(configured, out var cajaId) && cajaId > 0) return cajaId;
        return Settings.Default.CajaId > 0 ? Settings.Default.CajaId : 0;
    }

    private static string ReadTerminalId()
    {
        var configured = Environment.GetEnvironmentVariable("PAPELERIA_TERMINAL_ID");
        if (!string.IsNullOrWhiteSpace(configured)) return configured.Trim();
        return string.IsNullOrWhiteSpace(Settings.Default.TerminalId)
            ? Environment.MachineName
            : Settings.Default.TerminalId.Trim();
    }

    private static int ReadUserId(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return 0;

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');
            using var document = JsonDocument.Parse(Convert.FromBase64String(payload));

            foreach (var claimName in new[]
            {
                "nameid",
                "sub",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            })
            {
                if (document.RootElement.TryGetProperty(claimName, out var claim)
                    && int.TryParse(claim.GetString(), out var userId))
                {
                    return userId;
                }
            }
        }
        catch (Exception)
        {
        }

        return 0;
    }
}
