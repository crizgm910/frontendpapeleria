using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using PapeleriaDB.Models;

namespace PapeleriaDB.Services;

public sealed class TicketPrintingService
{
    private static readonly CultureInfo CurrencyCulture = CultureInfo.GetCultureInfo("es-MX");

    public bool Print(TicketData ticket)
    {
        var dialog = new PrintDialog();
        if (dialog.ShowDialog() != true) return false;

        var document = BuildDocument(ticket, dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
        dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, $"Ticket {ticket.Folio}");
        return true;
    }

    private static FlowDocument BuildDocument(TicketData ticket, double printableWidth, double printableHeight)
    {
        var width = Math.Min(printableWidth, 302d); // Aproximadamente 80 mm a 96 DPI.
        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 10,
            PageWidth = width,
            PageHeight = printableHeight,
            PagePadding = new Thickness(10),
            ColumnWidth = width - 20
        };

        document.Blocks.Add(Paragraph("PAPELERÍA DB", 16, FontWeights.Bold, TextAlignment.Center));
        document.Blocks.Add(Paragraph("Comprobante de venta", 10, FontWeights.SemiBold, TextAlignment.Center));
        document.Blocks.Add(Separator());
        document.Blocks.Add(Paragraph($"Folio: {ticket.Folio}"));
        document.Blocks.Add(Paragraph($"Fecha: {ticket.Fecha:dd/MM/yyyy HH:mm}"));
        document.Blocks.Add(Paragraph($"Pago: {ticket.MetodoPago}"));
        document.Blocks.Add(Paragraph($"Equipo: {ticket.Terminal}"));
        if (!string.IsNullOrWhiteSpace(ticket.Cliente))
            document.Blocks.Add(Paragraph($"Cliente: {ticket.Cliente.Trim()}"));
        document.Blocks.Add(Separator());

        var table = new Table { CellSpacing = 0 };
        table.Columns.Add(new TableColumn { Width = new GridLength(width * 0.48) });
        table.Columns.Add(new TableColumn { Width = new GridLength(width * 0.12) });
        table.Columns.Add(new TableColumn { Width = new GridLength(width * 0.20) });
        table.Columns.Add(new TableColumn { Width = new GridLength(width * 0.20) });
        var group = new TableRowGroup();
        table.RowGroups.Add(group);
        group.Rows.Add(Row("Artículo", "Cant.", "Precio", "Importe", true));
        foreach (var line in ticket.Lineas)
            group.Rows.Add(Row(line.Descripcion, line.Cantidad.ToString(), Money(line.PrecioUnitario), Money(line.Subtotal)));
        document.Blocks.Add(table);

        document.Blocks.Add(Separator());
        document.Blocks.Add(TotalLine("Subtotal", ticket.Subtotal));
        if (ticket.Descuento > 0) document.Blocks.Add(TotalLine("Descuento", -ticket.Descuento));
        document.Blocks.Add(TotalLine("TOTAL", ticket.Total, true));
        if (ticket.MontoRecibido.HasValue && ticket.MontoRecibido.Value > 0)
        {
            document.Blocks.Add(TotalLine("Recibido", ticket.MontoRecibido.Value));
            document.Blocks.Add(TotalLine("Cambio", Math.Max(0, ticket.MontoRecibido.Value - ticket.Total)));
        }
        document.Blocks.Add(Separator());
        document.Blocks.Add(Paragraph("¡Gracias por su compra!", 11, FontWeights.SemiBold, TextAlignment.Center));
        document.Blocks.Add(Paragraph("Conserve este ticket para cualquier aclaración.", 9, FontWeights.Normal, TextAlignment.Center));
        return document;
    }

    private static Paragraph Paragraph(string text, double size = 10, FontWeight? weight = null, TextAlignment alignment = TextAlignment.Left) =>
        new(new Run(text))
        {
            FontSize = size,
            FontWeight = weight ?? FontWeights.Normal,
            TextAlignment = alignment,
            Margin = new Thickness(0, 1, 0, 1)
        };

    private static Block Separator() => new Paragraph(new Run(new string('-', 42)))
    {
        TextAlignment = TextAlignment.Center,
        Margin = new Thickness(0, 3, 0, 3)
    };

    private static TableRow Row(string description, string quantity, string price, string subtotal, bool header = false)
    {
        var row = new TableRow { FontWeight = header ? FontWeights.Bold : FontWeights.Normal };
        row.Cells.Add(Cell(description, TextAlignment.Left));
        row.Cells.Add(Cell(quantity, TextAlignment.Center));
        row.Cells.Add(Cell(price, TextAlignment.Right));
        row.Cells.Add(Cell(subtotal, TextAlignment.Right));
        return row;
    }

    private static TableCell Cell(string text, TextAlignment alignment) => new(new Paragraph(new Run(text))
    {
        TextAlignment = alignment,
        Margin = new Thickness(1)
    })
    {
        Padding = new Thickness(1)
    };

    private static Paragraph TotalLine(string label, decimal amount, bool emphasis = false)
    {
        var paragraph = Paragraph(string.Empty, emphasis ? 13 : 10, emphasis ? FontWeights.Bold : FontWeights.Normal, TextAlignment.Right);
        paragraph.Inlines.Add(new Run($"{label}: {Money(amount)}"));
        return paragraph;
    }

    private static string Money(decimal amount) => amount.ToString("C2", CurrencyCulture);
}
