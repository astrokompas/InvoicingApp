using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using InvoicingApp.Models;
using System.Globalization;
using System.Windows.Media;
using System.Linq;

namespace InvoicingApp.Services
{
    public class ReportPDFService
    {
        private readonly ISettingsService _settingsService;

        public ReportPDFService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        public async Task<string> GenerateReportPdfAsync(ReportSummary report)
        {
            var settings = await _settingsService.GetSettingsAsync();

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Raport {report.StartDate:dd.MM.yyyy} - {report.EndDate:dd.MM.yyyy}";
            document.Info.Author = settings.CompanyName;
            document.Info.Subject = "Raport sprzedaży";

            // Create an empty page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Define fonts
            XFont titleFont = new XFont("Arial Bold", 18);
            XFont headerFont = new XFont("Arial Bold", 14);
            XFont normalFont = new XFont("Arial", 10);
            XFont boldFont = new XFont("Arial Bold", 10);
            XFont smallFont = new XFont("Arial", 8);

            // Define colors
            XColor mainColor = XColor.FromArgb(62, 108, 178);
            XColor blackColor = XColor.FromArgb(0, 0, 0);

            // Page margins
            double marginLeft = 50;
            double marginTop = 50;
            double marginRight = page.Width - 50;
            double availableWidth = marginRight - marginLeft;

            double currentY = marginTop;

            // Draw company logo
            if (!string.IsNullOrEmpty(settings.CompanyLogoPath) && File.Exists(settings.CompanyLogoPath))
            {
                try
                {
                    XImage logo = XImage.FromFile(settings.CompanyLogoPath);
                    double logoWidth = 150;
                    double logoHeight = logoWidth * logo.Height / logo.Width;
                    gfx.DrawImage(logo, marginLeft, currentY, logoWidth, logoHeight);
                    currentY += logoHeight + 10;
                }
                catch (Exception)
                {

                }
            }

            // Draw report title
            string title = $"Raport sprzedaży: {report.StartDate:dd.MM.yyyy} - {report.EndDate:dd.MM.yyyy}";
            gfx.DrawString(title, titleFont, new XSolidBrush(mainColor), marginLeft, currentY);
            currentY += 30;

            gfx.DrawLine(new XPen(mainColor, 1), marginLeft, currentY, marginRight, currentY);
            currentY += 15;

            // Draw company information
            gfx.DrawString("Dane firmy:", boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
            currentY += 15;
            gfx.DrawString(settings.CompanyName, normalFont, new XSolidBrush(blackColor), marginLeft, currentY);
            currentY += 15;
            gfx.DrawString(settings.CompanyAddress, normalFont, new XSolidBrush(blackColor), marginLeft, currentY);
            currentY += 15;
            gfx.DrawString($"NIP: {settings.CompanyTaxID}", normalFont, new XSolidBrush(blackColor), marginLeft, currentY);
            currentY += 30;

            // Draw summary section title
            gfx.DrawString("Podsumowanie:", headerFont, new XSolidBrush(mainColor), marginLeft, currentY);
            currentY += 25;

            // Draw summary data
            double summaryWidth = availableWidth / 3;

            // Column 1
            gfx.DrawString("Liczba faktur:", boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
            gfx.DrawString(report.TotalInvoices.ToString(), normalFont, new XSolidBrush(blackColor), marginLeft + 120, currentY);

            // Column 2
            gfx.DrawString("Zapłacone faktury:", boldFont, new XSolidBrush(blackColor), marginLeft + summaryWidth, currentY);
            gfx.DrawString(report.PaidInvoices.ToString(), normalFont, new XSolidBrush(blackColor), marginLeft + summaryWidth + 120, currentY);

            // Column 3
            gfx.DrawString("Niezapłacone faktury:", boldFont, new XSolidBrush(blackColor), marginLeft + 2 * summaryWidth, currentY);
            gfx.DrawString(report.UnpaidInvoices.ToString(), normalFont, new XSolidBrush(blackColor), marginLeft + 2 * summaryWidth + 120, currentY);

            currentY += 20;

            // Row 2
            gfx.DrawString("Kwota netto:", boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
            gfx.DrawString($"{report.TotalNetAmount:N2} {settings.Currency}", normalFont, new XSolidBrush(blackColor), marginLeft + 120, currentY);

            gfx.DrawString("Kwota VAT:", boldFont, new XSolidBrush(blackColor), marginLeft + summaryWidth, currentY);
            gfx.DrawString($"{report.TotalTaxAmount:N2} {settings.Currency}", normalFont, new XSolidBrush(blackColor), marginLeft + summaryWidth + 120, currentY);

            gfx.DrawString("Kwota brutto:", boldFont, new XSolidBrush(blackColor), marginLeft + 2 * summaryWidth, currentY);
            gfx.DrawString($"{report.TotalAmount:N2} {settings.Currency}", normalFont, new XSolidBrush(blackColor), marginLeft + 2 * summaryWidth + 120, currentY);

            currentY += 40;

            // Draw client breakdown section
            gfx.DrawString("Podział faktur wg klienta:", headerFont, new XSolidBrush(mainColor), marginLeft, currentY);
            currentY += 25;

            // Draw table header
            string[] headers = new string[] { "Klient", "Liczba faktur", "Kwota netto", "Kwota VAT", "Kwota brutto", "Zapłacone/Wszystkie" };
            double[] columnWidths = new double[] { availableWidth * 0.3, availableWidth * 0.1, availableWidth * 0.15, availableWidth * 0.15, availableWidth * 0.15, availableWidth * 0.15 };
            double rowHeight = 20;

            // Draw table header background
            double tableX = marginLeft;
            double tableY = currentY;

            XRect headerRect = new XRect(tableX, tableY, availableWidth, rowHeight);
            gfx.DrawRectangle(new XSolidBrush(mainColor), headerRect);

            // Draw header text
            double colX = tableX;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawString(headers[i], boldFont, new XSolidBrush(XColors.White), colX + 5, tableY + 15);
                colX += columnWidths[i];
            }

            tableY += rowHeight;

            // Draw client rows
            bool alternateRow = false;
            foreach (var client in report.ClientBreakdown)
            {
                if (tableY + rowHeight > page.Height - 100)
                {
                    // Add new page
                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    tableY = marginTop;

                    // Redraw header on new page
                    headerRect = new XRect(tableX, tableY, availableWidth, rowHeight);
                    gfx.DrawRectangle(new XSolidBrush(mainColor), headerRect);

                    colX = tableX;
                    for (int i = 0; i < headers.Length; i++)
                    {
                        gfx.DrawString(headers[i], boldFont, new XSolidBrush(XColors.White), colX + 5, tableY + 15);
                        colX += columnWidths[i];
                    }

                    tableY += rowHeight;
                }

                // Draw row background
                XRect rowRect = new XRect(tableX, tableY, availableWidth, rowHeight);
                if (alternateRow)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 240, 240)), rowRect);
                }
                else
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(255, 255, 255)), rowRect);
                }

                // Draw row border
                gfx.DrawRectangle(new XPen(XColor.FromArgb(200, 200, 200)), rowRect);

                // Draw client data
                colX = tableX;

                // Client name
                gfx.DrawString(client.ClientName, normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);
                colX += columnWidths[0];

                // Invoice count
                gfx.DrawString(client.TotalInvoices.ToString(), normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);
                colX += columnWidths[1];

                // Net amount
                gfx.DrawString($"{client.TotalNetAmount:N2}", normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);
                colX += columnWidths[2];

                // VAT amount
                gfx.DrawString($"{client.TotalTaxAmount:N2}", normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);
                colX += columnWidths[3];

                // Gross amount
                gfx.DrawString($"{client.TotalAmount:N2}", normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);
                colX += columnWidths[4];

                // Paid ratio
                gfx.DrawString($"{client.PaidInvoices}/{client.TotalInvoices}", normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);

                tableY += rowHeight;
                alternateRow = !alternateRow;
            }

            currentY = tableY + 30;

            // Add generation date
            gfx.DrawString($"Raport wygenerowany: {DateTime.Now:dd.MM.yyyy HH:mm}", smallFont, new XSolidBrush(blackColor), marginLeft, currentY);

            // Save the PDF to a temporary file
            string fileName = $"Raport_{report.StartDate:yyyy-MM-dd}_{report.EndDate:yyyy-MM-dd}";
            string tempFile = Path.Combine(Path.GetTempPath(), $"{fileName}.pdf");
            document.Save(tempFile);

            return tempFile;
        }
    }
}