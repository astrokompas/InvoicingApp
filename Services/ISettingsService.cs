using System.Threading.Tasks;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public interface ISettingsService
    {
        Task<AppSettings> GetSettingsAsync();
        Task SaveSettingsAsync(AppSettings settings);
        string[] GetVatRates();
        string[] GetPaymentMethods();
        int[] GetInvoiceRetentionOptions();
    }
}