using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using PapeleriaDB.Models;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PapeleriaDB.Services;

public class ReportExportService
{
    public void ExportarExcel(string ruta, IReadOnlyCollection<VentaDto> ventas)
    {
        using var documento = SpreadsheetDocument.Create(ruta, SpreadsheetDocumentType.Workbook);
        var libro = documento.AddWorkbookPart();
        libro.Workbook = new Workbook();

        var estilos = libro.AddNewPart<WorkbookStylesPart>();
        estilos.Stylesheet = CrearEstilos();
        estilos.Stylesheet.Save();

        var hoja = libro.AddNewPart<WorksheetPart>();
        var datos = new SheetData();
        hoja.Worksheet = new Worksheet(
            new SheetViews(new SheetView(
                new Pane { VerticalSplit = 1D, TopLeftCell = "A2", ActivePane = PaneValues.BottomLeft, State = PaneStateValues.Frozen })
            { WorkbookViewId = 0U }),
            new Columns(
                new Column { Min = 1, Max = 1, Width = 24, CustomWidth = true },
                new Column { Min = 2, Max = 2, Width = 16, CustomWidth = true },
                new Column { Min = 3, Max = 3, Width = 21, CustomWidth = true },
                new Column { Min = 4, Max = 4, Width = 15, CustomWidth = true },
                new Column { Min = 5, Max = 7, Width = 14, CustomWidth = true }),
            datos);

        datos.Append(FilaTexto(1, true, "Folio", "Método", "Fecha", "Estado", "Subtotal", "Descuento", "Total"));
        uint numeroFila = 2;
        foreach (var venta in ventas)
        {
            var fila = new Row { RowIndex = numeroFila++ };
            fila.Append(
                CeldaTexto(venta.Folio),
                CeldaTexto(venta.MetodoPago),
                CeldaTexto(venta.Fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
                CeldaTexto(venta.Estado),
                CeldaNumero(venta.Subtotal),
                CeldaNumero(venta.Descuento),
                CeldaNumero(venta.Total));
            AsignarReferencias(fila);
            datos.Append(fila);
        }

        var resumen = new Row { RowIndex = numeroFila };
        resumen.Append(CeldaTexto("TOTAL DEL REPORTE", 1), CeldaTexto(string.Empty), CeldaTexto(string.Empty), CeldaTexto(string.Empty),
            CeldaNumero(ventas.Sum(v => v.Subtotal), 3), CeldaNumero(ventas.Sum(v => v.Descuento), 3), CeldaNumero(ventas.Sum(v => v.Total), 3));
        AsignarReferencias(resumen);
        datos.Append(resumen);
        hoja.Worksheet.Append(new AutoFilter { Reference = $"A1:G{numeroFila - 1}" });

        var hojas = libro.Workbook.AppendChild(new Sheets());
        hojas.Append(new Sheet { Id = libro.GetIdOfPart(hoja), SheetId = 1, Name = "Ventas" });
        hoja.Worksheet.Save();
        libro.Workbook.Save();
    }

    public void ExportarPdf(string ruta, IReadOnlyCollection<VentaDto> ventas, string descripcionFiltro)
    {
        using var documento = new PdfDocument();
        documento.Info.Title = "Reporte de ventas - Papelería DB";
        documento.Info.Author = "Papelería DB";

        var fuenteTitulo = new XFont("Segoe UI", 18, XFontStyleEx.Bold);
        var fuenteSubtitulo = new XFont("Segoe UI", 9, XFontStyleEx.Regular);
        var fuenteEncabezado = new XFont("Segoe UI", 8, XFontStyleEx.Bold);
        var fuenteFila = new XFont("Segoe UI", 8, XFontStyleEx.Regular);
        var columnas = new[] { 42d, 148d, 84d, 126d, 76d, 84d, 84d };
        var titulos = new[] { "#", "Folio", "Método", "Fecha", "Estado", "Descuento", "Total" };
        const double margen = 32;
        const double altoFila = 22;

        PdfPage? pagina = null;
        XGraphics? grafico = null;
        double y = 0;

        void NuevaPagina()
        {
            grafico?.Dispose();
            pagina = documento.AddPage();
            pagina.Size = PageSize.A4;
            pagina.Orientation = PageOrientation.Landscape;
            grafico = XGraphics.FromPdfPage(pagina);
            y = margen;

            grafico.DrawString("Papelería DB · Reporte de ventas", fuenteTitulo, XBrushes.Black,
                new XRect(margen, y, pagina.Width.Point - margen * 2, 26), XStringFormats.TopLeft);
            y += 27;
            grafico.DrawString($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm} · {descripcionFiltro}", fuenteSubtitulo, XBrushes.DimGray,
                new XRect(margen, y, pagina.Width.Point - margen * 2, 18), XStringFormats.TopLeft);
            y += 24;

            var x = margen;
            for (var i = 0; i < titulos.Length; i++)
            {
                grafico.DrawRectangle(new XSolidBrush(XColor.FromArgb(37, 99, 235)), x, y, columnas[i], altoFila);
                grafico.DrawString(titulos[i], fuenteEncabezado, XBrushes.White,
                    new XRect(x + 5, y, columnas[i] - 10, altoFila), XStringFormats.CenterLeft);
                x += columnas[i];
            }
            y += altoFila;
        }

        NuevaPagina();
        var indice = 0;
        foreach (var venta in ventas)
        {
            if (pagina is null || grafico is null) break;
            if (y + altoFila + 45 > pagina.Height.Point - margen) NuevaPagina();
            indice++;
            var valores = new[]
            {
                indice.ToString(), venta.Folio, venta.MetodoPago,
                venta.Fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm"), venta.Estado,
                venta.Descuento.ToString("C", CultureInfo.CurrentCulture), venta.Total.ToString("C", CultureInfo.CurrentCulture)
            };
            var fondo = indice % 2 == 0 ? new XSolidBrush(XColor.FromArgb(248, 250, 252)) : XBrushes.White;
            var x = margen;
            for (var i = 0; i < valores.Length; i++)
            {
                grafico!.DrawRectangle(fondo, x, y, columnas[i], altoFila);
                grafico.DrawString(valores[i], fuenteFila, XBrushes.Black,
                    new XRect(x + 5, y, columnas[i] - 10, altoFila), XStringFormats.CenterLeft);
                x += columnas[i];
            }
            y += altoFila;
        }

        if (grafico is not null && pagina is not null)
        {
            y += 12;
            grafico.DrawLine(new XPen(XColor.FromArgb(203, 213, 225)), margen, y, pagina.Width.Point - margen, y);
            y += 10;
            grafico.DrawString($"Ventas: {ventas.Count}     Total: {ventas.Sum(v => v.Total):C}", fuenteEncabezado, XBrushes.Black,
                new XRect(margen, y, pagina.Width.Point - margen * 2, 20), XStringFormats.TopRight);
            grafico.Dispose();
        }

        documento.Save(ruta);
    }

    private static Row FilaTexto(uint indice, bool encabezado, params string[] valores)
    {
        var fila = new Row { RowIndex = indice };
        foreach (var valor in valores) fila.Append(CeldaTexto(valor, encabezado ? 1U : 0U));
        AsignarReferencias(fila);
        return fila;
    }

    private static void AsignarReferencias(Row fila)
    {
        var columna = 0;
        foreach (var celda in fila.Elements<Cell>())
        {
            celda.CellReference = $"{(char)('A' + columna)}{fila.RowIndex}";
            columna++;
        }
    }

    private static Cell CeldaTexto(string valor, uint estilo = 0) => new()
    {
        DataType = CellValues.InlineString,
        InlineString = new InlineString(new Text(valor)),
        StyleIndex = estilo
    };

    private static Cell CeldaNumero(decimal valor, uint estilo = 2) => new()
    {
        DataType = CellValues.Number,
        CellValue = new CellValue(valor.ToString(CultureInfo.InvariantCulture)),
        StyleIndex = estilo
    };

    private static Stylesheet CrearEstilos() => new(
        new NumberingFormats(
            new NumberingFormat { NumberFormatId = 164U, FormatCode = "\"$\"#,##0.00" }),
        new Fonts(
            new Font(),
            new Font(new Bold(), new Color { Rgb = "FFFFFFFF" })),
        new Fills(
            new Fill(new PatternFill { PatternType = PatternValues.None }),
            new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
            new Fill(new PatternFill(new ForegroundColor { Rgb = "FF2563EB" }) { PatternType = PatternValues.Solid })),
        new Borders(new Border()),
        new CellStyleFormats(new CellFormat()),
        new CellFormats(
            new CellFormat(),
            new CellFormat { FontId = 1, FillId = 2, ApplyFill = true, ApplyFont = true },
            new CellFormat { NumberFormatId = 164U, ApplyNumberFormat = true },
            new CellFormat { FontId = 1, FillId = 2, NumberFormatId = 164U, ApplyFill = true, ApplyFont = true, ApplyNumberFormat = true }));
}
