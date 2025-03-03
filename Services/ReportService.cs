using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public class ReportService : IReportService
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IClientService _clientService;

        public ReportService(IInvoiceService invoiceService, IClientService clientService)
        {
            _invoiceService = invoiceService;
            _clientService = clientService;
        }

        public async Task<ReportSummary> GenerateReportAsync(DateTime? startDate = null, DateTime? endDate = null, string clientId = null)
        {
            startDate = startDate ?? DateTime.MinValue;
            endDate = endDate ?? DateTime.MaxValue;

            // Get all invoices within the date range
            var allInvoices = await _invoiceService.GetAllInvoicesAsync();

            var filteredInvoices = allInvoices
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
                .ToList();

            // Apply client filter if specified
            if (!string.IsNullOrEmpty(clientId))
            {
                filteredInvoices = filteredInvoices
                    .Where(i => i.ClientId == clientId)
                    .ToList();
            }

            // Create report summary
            var summary = new ReportSummary
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                ClientId = clientId,
                TotalInvoices = filteredInvoices.Count,
                TotalAmount = filteredInvoices.Sum(i => i.TotalGross),
                TotalNetAmount = filteredInvoices.Sum(i => i.TotalNet),
                TotalTaxAmount = filteredInvoices.Sum(i => i.TotalVat),
                PaidInvoices = filteredInvoices.Count(i => i.IsPaid),
                UnpaidInvoices = filteredInvoices.Count(i => !i.IsPaid)
            };

            // Generate client breakdown
            var clientGroups = filteredInvoices
                .GroupBy(i => i.ClientId)
                .ToList();

            foreach (var group in clientGroups)
            {
                var client = await _clientService.GetClientByIdAsync(group.Key);
                if (client == null) continue;

                var clientSummary = new ClientReportSummary
                {
                    ClientId = client.Id,
                    ClientName = client.Name,
                    TotalInvoices = group.Count(),
                    TotalAmount = group.Sum(i => i.TotalGross),
                    TotalNetAmount = group.Sum(i => i.TotalNet),
                    TotalTaxAmount = group.Sum(i => i.TotalVat),
                    PaidInvoices = group.Count(i => i.IsPaid),
                    UnpaidInvoices = group.Count(i => !i.IsPaid)
                };

                summary.ClientBreakdown.Add(clientSummary);
            }

            return summary;
        }
    }
}