using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using InvoicingApp.DataStorage;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IDataStorage<Invoice> _invoiceStorage;
        private readonly ISettingsService _settingsService;

        public InvoiceService(IDataStorage<Invoice> invoiceStorage, ISettingsService settingsService)
        {
            _invoiceStorage = invoiceStorage;
            _settingsService = settingsService;
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _invoiceStorage.GetAllAsync();
        }

        public async Task<Invoice> GetInvoiceByIdAsync(string id)
        {
            return await _invoiceStorage.GetByIdAsync(id);
        }

        public async Task SaveInvoiceAsync(Invoice invoice)
        {
            await _invoiceStorage.SaveAsync(invoice);
        }

        public async Task DeleteInvoiceAsync(string id)
        {
            await _invoiceStorage.DeleteAsync(id);
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByClientIdAsync(string clientId)
        {
            return await _invoiceStorage.QueryAsync(i => i.ClientId == clientId);
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _invoiceStorage.QueryAsync(i =>
                i.InvoiceDate >= startDate.Date &&
                i.InvoiceDate <= endDate.Date);
        }

        public async Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync()
        {
            return await _invoiceStorage.QueryAsync(i => !i.IsPaid);
        }

        public string GenerateNextInvoiceNumber()
        {
            // Get all invoices and find the latest one
            var allInvoices = _invoiceStorage.GetAllAsync().Result.ToList();

            // Default format: FV/001/YYYY
            string prefix = "FV";
            int sequenceNumber = 1;
            int year = DateTime.Now.Year;

            if (allInvoices.Count > 0)
            {
                // Extract the sequence number from the latest invoice
                // Assuming format like "FV/123/2025"
                var latestInvoice = allInvoices
                    .Where(i => i.InvoiceNumber.Contains($"/{year}"))
                    .OrderByDescending(i => i.InvoiceDate)
                    .FirstOrDefault();

                if (latestInvoice != null)
                {
                    // Parse the sequence number
                    var match = Regex.Match(latestInvoice.InvoiceNumber, @"(\w+)/(\d+)/(\d+)");
                    if (match.Success && match.Groups.Count >= 3)
                    {
                        prefix = match.Groups[1].Value;
                        if (int.TryParse(match.Groups[2].Value, out int lastSequence))
                        {
                            sequenceNumber = lastSequence + 1;
                        }
                    }
                }
            }

            // Format: FV/001/2025
            return $"{prefix}/{sequenceNumber:D3}/{year}";
        }

        public async Task PurgeOldInvoicesAsync()
        {
            var settings = await _settingsService.GetSettingsAsync();

            // Skip if retention setting is "never"
            if (settings.InvoiceRetentionDays <= 0)
            {
                return;
            }

            var cutoffDate = DateTime.Now.AddDays(-settings.InvoiceRetentionDays);
            var oldInvoices = await _invoiceStorage.QueryAsync(i => i.InvoiceDate < cutoffDate);

            foreach (var invoice in oldInvoices)
            {
                await _invoiceStorage.DeleteAsync(invoice.Id);
            }
        }
    }
}