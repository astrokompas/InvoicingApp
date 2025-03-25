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
        private readonly IDataStorage<Client> _clientStorage;
        private readonly ISettingsService _settingsService;

        public InvoiceService(
            IDataStorage<Invoice> invoiceStorage,
            IDataStorage<Client> clientStorage,
            ISettingsService settingsService)
        {
            _invoiceStorage = invoiceStorage;
            _clientStorage = clientStorage;
            _settingsService = settingsService;
        }

        public async Task<Invoice> GetInvoiceWithClientAsync(string id)
        {
            var invoice = await GetInvoiceByIdAsync(id);
            if (invoice != null && !string.IsNullOrEmpty(invoice.ClientId))
            {
                // Load the client data
                invoice.Client = await _clientStorage.GetByIdAsync(invoice.ClientId);
            }
            return invoice;
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesWithClientsAsync()
        {
            var invoices = await _invoiceStorage.GetAllAsync();
            var clients = await _clientStorage.GetAllAsync();

            // Create a dictionary for faster lookups
            var clientDict = clients.ToDictionary(c => c.Id);

            foreach (var invoice in invoices)
            {
                if (!string.IsNullOrEmpty(invoice.ClientId) && clientDict.TryGetValue(invoice.ClientId, out var client))
                {
                    invoice.Client = client;
                }
            }

            return invoices;
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            try
            {
                var invoices = await _invoiceStorage.GetAllAsync();
                var clientIds = invoices
                    .Where(i => !string.IsNullOrEmpty(i.ClientId))
                    .Select(i => i.ClientId)
                    .Distinct()
                    .ToList();

                // Load all required clients in one go
                var clientTasks = clientIds.Select(id => _clientStorage.GetByIdAsync(id));
                var clients = await Task.WhenAll(clientTasks);

                // Create a lookup dictionary
                var clientDict = clients
                    .Where(c => c != null)
                    .ToDictionary(c => c.Id);

                // Assign clients to invoices
                foreach (var invoice in invoices)
                {
                    if (!string.IsNullOrEmpty(invoice.ClientId) &&
                        clientDict.TryGetValue(invoice.ClientId, out var client))
                    {
                        invoice.Client = client;
                    }
                }

                return invoices;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all invoices: {ex.Message}");
                return new List<Invoice>();
            }
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

        public async Task<string> GenerateNextInvoiceNumberAsync()
        {
            // Get all invoices asynchronously
            var allInvoices = await _invoiceStorage.GetAllAsync();

            // Format: FV/NNN/MM/YYYY
            string prefix = "FV";
            int sequenceNumber = 1;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            if (allInvoices.Any())
            {
                // Extract the sequence number from the latest invoice in the current month
                var currentMonthInvoices = allInvoices
                    .Where(i => i.InvoiceNumber.Contains($"/{month.ToString("00")}/{year}"))
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToList();

                if (currentMonthInvoices.Any())
                {
                    // Parse the sequence number
                    var match = Regex.Match(currentMonthInvoices.First().InvoiceNumber, @"(\w+)/(\d+)/(\d+)/(\d+)");
                    if (match.Success && match.Groups.Count >= 5)
                    {
                        prefix = match.Groups[1].Value;
                        if (int.TryParse(match.Groups[2].Value, out int lastSequence))
                        {
                            sequenceNumber = lastSequence + 1;
                        }
                    }
                }
            }

            // Format: FV/001/03/2025
            return $"{prefix}/{sequenceNumber:D3}/{month:D2}/{year}";
        }

        public string GenerateNextInvoiceNumber()
        {
            try
            {
                // Create a new task and run it on a different thread
                return Task.Run(() => GenerateNextInvoiceNumberAsync()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating invoice number: {ex.Message}");
                return $"FV/001/{DateTime.Now.Year}";
            }
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