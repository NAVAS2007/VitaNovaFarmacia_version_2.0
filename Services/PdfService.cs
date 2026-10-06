using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Services;

public class PdfService
{
    private readonly FarmaciaDbContext _context;

    // Paleta de marca 
    private static readonly DeviceRgb AzulOscuro = new DeviceRgb(10, 30, 61);
    private static readonly DeviceRgb AzulMedio = new DeviceRgb(26, 58, 107);
    private static readonly DeviceRgb AzulClaro = new DeviceRgb(77, 139, 247);
    private static readonly DeviceRgb GrisClaro = new DeviceRgb(240, 244, 248);
    private static readonly DeviceRgb GrisBorde = new DeviceRgb(221, 228, 236);
    private static readonly DeviceRgb Verde = new DeviceRgb(76, 175, 132);
    private static readonly DeviceRgb Blanco = new DeviceRgb(255, 255, 255);

    // Fuentes (evita depender de SetBold(), que no está disponible en todas las versiones de iText)
    private static readonly PdfFont FuenteBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);


    public PdfService(FarmaciaDbContext context)
    {
        _context = context;
    }

    public byte[] GenerarFacturaPdf(int ventaId)
    {
        var venta = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Empleado)
            .Include(v => v.MetodoPago)
            .Include(v => v.DetalleVentas)
            .ThenInclude(dv => dv.Producto)
            .FirstOrDefault(v => v.ID == ventaId);

        if (venta == null)
            throw new Exception($"Venta con ID {ventaId} no encontrada");

        using (var stream = new MemoryStream())
        {
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            document.SetMargins(0, 36, 36, 36); // el header va sin margen superior

            try
            {
                // HEADER con banda de colo
                var headerTable = new Table(1).UseAllAvailableWidth();
                var headerCell = new Cell()
                    .SetBackgroundColor(AzulOscuro)
                    .SetBorder(null)
                    .SetPadding(20)
                    .SetTextAlignment(TextAlignment.CENTER);

                headerCell.Add(new Paragraph("+ VITA NOVA FARMACIA")
                    .SetFont(FuenteBold).SetFontColor(Blanco).SetFontSize(22));
                headerCell.Add(new Paragraph("Su farmacia de confianza")
                    .SetFontColor(new DeviceRgb(200, 214, 229)).SetFontSize(11).SetMarginTop(2));
                headerCell.Add(new Paragraph("FACTURA DE VENTA")
                    .SetFont(FuenteBold).SetFontColor(AzulClaro).SetFontSize(13).SetMarginTop(8));

                headerTable.AddCell(headerCell);
                document.Add(headerTable);

                document.Add(new Paragraph(" ").SetMarginBottom(5));

                var contentWrapper = new Div().SetPaddingLeft(20).SetPaddingRight(20).SetPaddingTop(15);

                // Info de factura 
                var infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 })).UseAllAvailableWidth();
                infoTable.AddCell(CeldaLabel("Número de Factura"));
                infoTable.AddCell(CeldaValor(venta.ID.ToString()));
                infoTable.AddCell(CeldaLabel("Fecha"));
                infoTable.AddCell(CeldaValor(venta.Fecha.ToString("dd/MM/yyyy HH:mm")));
                infoTable.AddCell(CeldaLabel("Empleado"));
                infoTable.AddCell(CeldaValor(venta.Empleado?.Nombre ?? "N/A"));
                contentWrapper.Add(infoTable.SetMarginBottom(15));

                // Datos del cliente (caja gris) 
                var clienteBox = new Div()
                    .SetBackgroundColor(GrisClaro)
                    .SetPadding(12)
                    .SetMarginBottom(15);

                clienteBox.Add(new Paragraph("DATOS DEL CLIENTE")
                    .SetFont(FuenteBold).SetFontColor(AzulOscuro).SetFontSize(11).SetMarginBottom(6));

                var clienteTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 })).UseAllAvailableWidth();
                clienteTable.AddCell(CeldaLabel("Nombre"));
                clienteTable.AddCell(CeldaValor($"{venta.Cliente?.Nombre ?? "N/A"} {venta.Cliente?.Apellido ?? ""}"));
                clienteTable.AddCell(CeldaLabel("Documento"));
                clienteTable.AddCell(CeldaValor(venta.Cliente?.Documento ?? "N/A"));
                clienteTable.AddCell(CeldaLabel("Teléfono"));
                clienteTable.AddCell(CeldaValor(venta.Cliente?.Telefono ?? "N/A"));
                clienteTable.AddCell(CeldaLabel("Correo"));
                clienteTable.AddCell(CeldaValor(venta.Cliente?.Correo ?? "N/A"));
                clienteBox.Add(clienteTable);
                contentWrapper.Add(clienteBox);

                // Tabla de producto
                contentWrapper.Add(new Paragraph("DETALLE DE PRODUCTOS")
                    .SetFont(FuenteBold).SetFontColor(AzulOscuro).SetFontSize(11).SetMarginBottom(6));

                var table = new Table(UnitValue.CreatePercentArray(new float[] { 4, 1.5f, 2, 2 })).UseAllAvailableWidth();

                foreach (var titulo in new[] { "Producto", "Cantidad", "Precio Unit.", "Subtotal" })
                {
                    table.AddHeaderCell(new Cell()
                        .SetBackgroundColor(AzulOscuro)
                        .SetPadding(8)
                        .Add(new Paragraph(titulo).SetFont(FuenteBold).SetFontColor(Blanco).SetFontSize(10)));
                }

                bool alterno = false;
                foreach (var detalle in venta.DetalleVentas)
                {
                    var fondoFila = alterno ? GrisClaro : Blanco;
                    table.AddCell(FilaCelda(detalle.Producto?.Nombre ?? "N/A", fondoFila, TextAlignment.LEFT));
                    table.AddCell(FilaCelda(detalle.Cantidad.ToString(), fondoFila, TextAlignment.CENTER));
                    table.AddCell(FilaCelda($"${detalle.PrecioUnitario:F2}", fondoFila, TextAlignment.RIGHT));
                    table.AddCell(FilaCelda($"${(detalle.Cantidad * detalle.PrecioUnitario):F2}", fondoFila, TextAlignment.RIGHT));
                    alterno = !alterno;
                }

                contentWrapper.Add(table.SetMarginBottom(20));

                // Total 
                var totalsBox = new Div()
                    .SetWidth(220)
                    .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                    .SetMarginBottom(20);

                var totalsInner = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 })).UseAllAvailableWidth();

                var totalLabelCell = new Cell().SetBorder(null).SetBackgroundColor(AzulOscuro).SetPadding(10)
                    .Add(new Paragraph("TOTAL").SetFont(FuenteBold).SetFontColor(Blanco).SetFontSize(14));
                var totalValueCell = new Cell().SetBorder(null).SetBackgroundColor(AzulOscuro).SetPadding(10)
                    .Add(new Paragraph($"${venta.Total:F2}").SetFont(FuenteBold).SetFontColor(Blanco).SetFontSize(14))
                    .SetTextAlignment(TextAlignment.RIGHT);

                totalsInner.AddCell(totalLabelCell);
                totalsInner.AddCell(totalValueCell);

                totalsBox.Add(totalsInner);
                contentWrapper.Add(totalsBox);

                //  Info de pago
                var paymentTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 })).UseAllAvailableWidth();
                paymentTable.AddCell(CeldaLabel("Método de Pago"));
                paymentTable.AddCell(CeldaValor(venta.MetodoPago?.Nombre ?? "N/A"));
                paymentTable.AddCell(CeldaLabel("Monto Recibido"));
                paymentTable.AddCell(CeldaValor(venta.Recibido.HasValue ? $"${venta.Recibido:F2}" : "N/A"));
                paymentTable.AddCell(CeldaLabel("Cambio"));
                paymentTable.AddCell(new Cell().SetBorder(null).Add(
                    new Paragraph(venta.Cambio.HasValue ? $"${venta.Cambio:F2}" : "N/A")
                        .SetFont(FuenteBold).SetFontColor(Verde)));
                contentWrapper.Add(paymentTable.SetMarginBottom(10));

                document.Add(contentWrapper);

                // Footer con banda de color 
                var footerTable = new Table(1).UseAllAvailableWidth();
                var footerCell = new Cell()
                    .SetBackgroundColor(AzulOscuro)
                    .SetBorder(null)
                    .SetPadding(15)
                    .SetTextAlignment(TextAlignment.CENTER);
                footerCell.Add(new Paragraph("Gracias por su compra. ¡Vuelva pronto!")
                    .SetFontColor(Blanco).SetFontSize(11));
                footerTable.AddCell(footerCell);
                document.Add(footerTable);

                document.Close();
            }
            finally
            {
                document?.Close();
            }

            return stream.ToArray();
        }
    }

    // Helpers de estilo 
    private static Cell CeldaLabel(string texto) =>
        new Cell().SetBorder(null).Add(new Paragraph(texto).SetFont(FuenteBold).SetFontColor(AzulOscuro).SetFontSize(10));

    private static Cell CeldaValor(string texto) =>
        new Cell().SetBorder(null).Add(new Paragraph(texto).SetFontSize(10));

    private static Cell FilaCelda(string texto, DeviceRgb fondo, TextAlignment alineacion) =>
        new Cell().SetBackgroundColor(fondo).SetPadding(6).SetTextAlignment(alineacion)
            .Add(new Paragraph(texto).SetFontSize(10));
}