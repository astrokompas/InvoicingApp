using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvoicingApp.Models;
namespace InvoicingApp.Services
{
    public interface IInvoiceService
    {
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<Invoice> GetInvoiceByIdAsync(string id);
        Task SaveInvoiceAsync(Invoice invoice);
        Task DeleteInvoiceAsync(string id);
        Task<IEnumerable<Invoice>> GetInvoicesByClientIdAsync(string clientId);
        Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync();
        string GenerateNextInvoiceNumber();
        Task<string> GenerateNextInvoiceNumberAsync();
        Task PurgeOldInvoicesAsync();
    }
}