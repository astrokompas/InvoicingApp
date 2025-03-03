using System.Threading.Tasks;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public interface IPDFService
    {
        Task<string> GenerateInvoicePdfAsync(Invoice invoice);
    }
}