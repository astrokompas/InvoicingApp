using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using InvoicingApp.Models;
using System.Globalization;
using System.Windows.Media;
using System.Xml.Linq;
using System.Drawing;

namespace InvoicingApp.Services
{
    public class PDFService : IPDFService
    {
        private readonly ISettingsService _settingsService;

        public PDFService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        public async Task<string> GenerateInvoicePdfAsync(Invoice invoice)
        {
            var settings = await _settingsService.GetSettingsAsync();

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Faktura {invoice.InvoiceNumber}";
            document.Info.Author = settings.CompanyName;
            document.Info.Subject = "Faktura VAT";

            // Create an empty page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Define fonts
            XFont headerFont = new XFont("Arial Bold", 16);
            XFont normalFont = new XFont("Arial", 10);
            XFont boldFont = new XFont("Arial Bold", 10);
            XFont smallFont = new XFont("Arial", 8);

            // Define colors
            XColor mainColor = XColor.FromArgb(62, 108, 178); // #3E6CB2
            XColor blackColor = XColor.FromArgb(0, 0, 0);

            // Page margins
            double marginLeft = 50;
            double marginTop = 50;
            double marginRight = page.Width - 50;
            double availableWidth = marginRight - marginLeft;

            double currentY = marginTop;

            // Draw company logo if available
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
                    // If loading the logo fails, just skip it
                }
            }

            // Draw "FAKTURA VAT" header
            gfx.DrawString("FAKTURA VAT", headerFont, new XSolidBrush(mainColor), marginLeft, currentY);
            gfx.DrawString(invoice.InvoiceNumber, headerFont, new XSolidBrush(mainColor), marginRight, currentY, XStringFormats.TopRight);
            currentY += 30;

            // Draw horizontal line
            gfx.DrawLine(new XPen(mainColor, 1), marginLeft, currentY, marginRight, currentY);
            currentY += 15;

            // Draw issue date and other dates
            gfx.DrawString("Data wystawienia:", boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
            gfx.DrawString(invoice.InvoiceDate.ToString("dd.MM.yyyy"), normalFont, new XSolidBrush(blackColor), marginLeft + 120, currentY);
            currentY += 15;

            gfx.DrawString("Data sprzedaży:", boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
            gfx.DrawString(invoice.SellingDate.ToString("dd.MM.yyyy"), normalFont, new XSolidBrush(blackColor), marginLeft + 120, currentY);
            currentY += 15;

            gfx.DrawString("Termin płatności:", boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
            gfx.DrawString(invoice.DueDate.ToString("dd.MM.yyyy"), normalFont, new XSolidBrush(blackColor), marginLeft + 120, currentY);
            currentY += 30;

            // Draw seller and buyer info
            double columnWidth = availableWidth / 2 - 10;
            double rightColumn = marginLeft + columnWidth + 20;
            double sellerBuyerBoxTop = currentY;

            // Seller box
            gfx.DrawString("Sprzedawca:", boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
            currentY += 15;
            gfx.DrawString(settings.CompanyName, boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
            currentY += 15;
            gfx.DrawString(settings.CompanyAddress, normalFont, new XSolidBrush(blackColor), marginLeft, currentY);
            currentY += 15;
            gfx.DrawString($"NIP: {settings.CompanyTaxID}", normalFont, new XSolidBrush(blackColor), marginLeft, currentY);
            if (!string.IsNullOrEmpty(settings.CompanyPhone))
            {
                currentY += 15;
                gfx.DrawString($"Tel: {settings.CompanyPhone}", normalFont, new XSolidBrush(blackColor), marginLeft, currentY);
            }
            if (!string.IsNullOrEmpty(settings.CompanyEmail))
            {
                currentY += 15;
                gfx.DrawString($"Email: {settings.CompanyEmail}", normalFont, new XSolidBrush(blackColor), marginLeft, currentY);
            }

            // Reset Y position for buyer
            currentY = sellerBuyerBoxTop;

            // Buyer box
            gfx.DrawString("Nabywca:", boldFont, new XSolidBrush(blackColor), rightColumn, currentY);
            currentY += 15;
            gfx.DrawString(invoice.Client.Name, boldFont, new XSolidBrush(blackColor), rightColumn, currentY);
            currentY += 15;
            gfx.DrawString(invoice.Client.Address, normalFont, new XSolidBrush(blackColor), rightColumn, currentY);
            currentY += 15;
            gfx.DrawString($"NIP: {invoice.Client.TaxID}", normalFont, new XSolidBrush(blackColor), rightColumn, currentY);
            if (!string.IsNullOrEmpty(invoice.Client.Phone))
            {
                currentY += 15;
                gfx.DrawString($"Tel: {invoice.Client.Phone}", normalFont, new XSolidBrush(blackColor), rightColumn, currentY);
            }
            if (!string.IsNullOrEmpty(invoice.Client.Email))
            {
                currentY += 15;
                gfx.DrawString($"Email: {invoice.Client.Email}", normalFont, new XSolidBrush(blackColor), rightColumn, currentY);
            }

            // Move to the lower position of both columns
            currentY = Math.Max(currentY, sellerBuyerBoxTop) + 30;

            // Draw invoice items table
            double[] columnWidths = new double[] { 30, 220, 50, 70, 50, 80 };
            string[] headers = new string[] { "Lp.", "Nazwa", "Ilość", "Cena netto", "VAT", "Wartość" };
            double rowHeight = 20;

            // Table header
            double tableX = marginLeft;
            double tableY = currentY;

            // Draw table header background
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

            // Draw items
            bool alternateRow = false;
            for (int i = 0; i < invoice.Items.Count; i++)
            {
                var item = invoice.Items[i];

                // Check if we need to add a new page
                if (tableY + rowHeight > page.Height - 100)
                {
                    // Add new page
                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    tableY = marginTop;

                    // Redraw header on new page
                    colX = tableX;
                    headerRect = new XRect(tableX, tableY, availableWidth, rowHeight);
                    gfx.DrawRectangle(new XSolidBrush(mainColor), headerRect);

                    for (int j = 0; j < headers.Length; j++)
                    {
                        gfx.DrawString(headers[j], boldFont, new XSolidBrush(XColors.White), colX + 5, tableY + 15);
                        colX += columnWidths[j];
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

                // Draw item data
                colX = tableX;

                // Lp.
                gfx.DrawString((i + 1).ToString(), normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);
                colX += columnWidths[0];

                // Name
                gfx.DrawString(item.Description, normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);
                colX += columnWidths[1];

                // Quantity
                gfx.DrawString(item.Quantity.ToString("0.##"), normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);
                colX += columnWidths[2];

                // Net price
                gfx.DrawString(item.NetPrice.ToString("0.00"), normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);
                colX += columnWidths[3];

                // VAT
                gfx.DrawString(item.VatRate, normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);
                colX += columnWidths[4];

                // Total
                gfx.DrawString(item.TotalGross.ToString("0.00"), normalFont, new XSolidBrush(blackColor), colX + 5, tableY + 15);

                tableY += rowHeight;
                alternateRow = !alternateRow;
            }

            // Draw totals
            currentY = tableY + 20;

            // Draw summary box
            XRect summaryRect = new XRect(marginRight - 200, currentY, 200, 80);
            gfx.DrawRectangle(new XPen(mainColor), summaryRect);

            // Draw summary content
            gfx.DrawString("Razem netto:", boldFont, new XSolidBrush(blackColor), marginRight - 190, currentY + 20);
            gfx.DrawString(invoice.TotalNet.ToString("0.00") + " " + settings.Currency, normalFont, new XSolidBrush(blackColor), marginRight - 10, currentY + 20, XStringFormats.TopRight);

            gfx.DrawString("Razem VAT:", boldFont, new XSolidBrush(blackColor), marginRight - 190, currentY + 40);
            gfx.DrawString(invoice.TotalVat.ToString("0.00") + " " + settings.Currency, normalFont, new XSolidBrush(blackColor), marginRight - 10, currentY + 40, XStringFormats.TopRight);

            gfx.DrawString("Do zapłaty:", boldFont, new XSolidBrush(mainColor), marginRight - 190, currentY + 60);
            gfx.DrawString(invoice.TotalGross.ToString("0.00") + " " + settings.Currency, boldFont, new XSolidBrush(mainColor), marginRight - 10, currentY + 60, XStringFormats.TopRight);

            currentY += 100;

            // Payment information
            gfx.DrawString("Forma płatności:", boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
            gfx.DrawString(invoice.PaymentMethod, normalFont, new XSolidBrush(blackColor), marginLeft + 120, currentY);
            currentY += 20;

            if (invoice.PaymentMethod == "Przelew" && !string.IsNullOrEmpty(invoice.BankAccount))
            {
                gfx.DrawString("Numer konta:", boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
                gfx.DrawString(invoice.BankAccount, normalFont, new XSolidBrush(blackColor), marginLeft + 120, currentY);
                currentY += 20;
            }

            // Additional notes
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                gfx.DrawString("Uwagi:", boldFont, new XSolidBrush(blackColor), marginLeft, currentY);
                currentY += 15;

                // Split notes into lines to fit the page width
                foreach (var line in SplitTextToFitWidth(invoice.Notes, normalFont, availableWidth, gfx))
                {
                    gfx.DrawString(line, normalFont, new XSolidBrush(blackColor), marginLeft, currentY);
                    currentY += 15;
                }
            }

            // Save the PDF to a temporary file
            string tempFile = Path.Combine(Path.GetTempPath(), $"Faktura_{invoice.InvoiceNumber.Replace("/", "_")}.pdf");
            document.Save(tempFile);

            return tempFile;
        }

        private List<string> SplitTextToFitWidth(string text, XFont font, double maxWidth, XGraphics gfx)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');
            string currentLine = "";

            foreach (var word in words)
            {
                string testLine = currentLine.Length > 0 ? currentLine + " " + word : word;
                XSize size = gfx.MeasureString(testLine, font);

                if (size.Width <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            return lines;
        }
    }
}