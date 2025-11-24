using NextPapyros.Application.Reports;
using NextPapyros.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NextPapyros.Infrastructure.Reports;

/// <summary>
/// Implementación del servicio de generación de comprobantes usando QuestPDF.
/// </summary>
public class ComprobanteService : IComprobanteService
{
    /// <inheritdoc />
    public byte[] GenerarComprobantePdf(Venta venta)
    {
        // Configurar licencia de QuestPDF (Community para uso no comercial)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeContent(content, venta));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("NextPapyros").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                column.Item().Text("Papelería y Suministros").FontSize(12);
                column.Item().Text("NIT: 900.123.456-7").FontSize(10);
                column.Item().Text("Tel: (601) 234-5678").FontSize(10);
            });

            row.RelativeItem().Column(column =>
            {
                column.Item().AlignRight().Text("COMPROBANTE DE VENTA").FontSize(14).Bold();
            });
        });
    }

    private void ComposeContent(IContainer container, Venta venta)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(10);

            // Información de la venta
            column.Item().Element(cont => ComposeInfo(cont, venta));

            // Tabla de productos
            column.Item().Element(cont => ComposeTable(cont, venta));

            // Totales
            column.Item().Element(cont => ComposeTotales(cont, venta));

            // Pie del comprobante
            column.Item().PaddingTop(20).Text("Gracias por su compra").FontSize(12).Bold().AlignCenter();
            column.Item().Text("Este documento es un comprobante de venta válido").FontSize(8).AlignCenter();
        });
    }

    private void ComposeInfo(IContainer container, Venta venta)
    {
        container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Comprobante No: {venta.Id:D8}").Bold();
                row.RelativeItem().AlignCenter().Text($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}");
                row.RelativeItem().AlignRight().Text($"Estado: {venta.Estado}").Bold();
            });
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Método de pago: {venta.MetodoPago}").Bold();
            });
        });
    }

    private void ComposeTable(IContainer container, Venta venta)
    {
        container.Table(table =>
        {
            // Definir columnas
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1); // Cantidad
                columns.RelativeColumn(3); // Producto
                columns.RelativeColumn(2); // Código
                columns.RelativeColumn(2); // Precio Unit.
                columns.RelativeColumn(2); // Subtotal
            });

            // Encabezado
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Cant.").Bold();
                header.Cell().Element(CellStyle).Text("Producto").Bold();
                header.Cell().Element(CellStyle).Text("Código").Bold();
                header.Cell().Element(CellStyle).AlignRight().Text("Precio Unit.").Bold();
                header.Cell().Element(CellStyle).AlignRight().Text("Subtotal").Bold();

                static IContainer CellStyle(IContainer c) =>
                    c.DefaultTextStyle(x => x.FontSize(10))
                     .PaddingVertical(5)
                     .BorderBottom(1)
                     .BorderColor(Colors.Black);
            });

            // Filas
            foreach (var linea in venta.Lineas)
            {
                table.Cell().Element(CellStyle).Text(linea.Cantidad.ToString());
                table.Cell().Element(CellStyle).Text(linea.Producto?.Nombre ?? "Producto");
                table.Cell().Element(CellStyle).Text(linea.ProductoCodigo);
                table.Cell().Element(CellStyle).AlignRight().Text($"${linea.PrecioUnitario:N2}");
                table.Cell().Element(CellStyle).AlignRight().Text($"${linea.Subtotal:N2}");

                static IContainer CellStyle(IContainer c) =>
                    c.PaddingVertical(5)
                     .BorderBottom(0.5f)
                     .BorderColor(Colors.Grey.Lighten2);
            }
        });
    }

    private void ComposeTotales(IContainer container, Venta venta)
    {
        container.AlignRight().Column(column =>
        {
            column.Item().PaddingTop(10).Row(row =>
            {
                row.ConstantItem(150).Text("Total:").FontSize(14).Bold();
                row.ConstantItem(100).AlignRight().Text($"${venta.Total:N2}").FontSize(14).Bold();
            });
        });
    }
}
