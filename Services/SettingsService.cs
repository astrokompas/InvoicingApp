using System.Threading.Tasks;
using InvoicingApp.DataStorage;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IDataStorage<AppSettings> _settingsStorage;
        private AppSettings _cachedSettings;

        public SettingsService(IDataStorage<AppSettings> settingsStorage)
        {
            _settingsStorage = settingsStorage;
        }

        public async Task<AppSettings> GetSettingsAsync()
        {
            if (_cachedSettings != null)
            {
                return _cachedSettings;
            }

            var settings = await _settingsStorage.GetByIdAsync("settings");

            if (settings == null)
            {
                // Create default settings
                settings = new AppSettings
                {
                    Id = "settings",
                    CompanyName = "Moja Firma",
                    CompanyAddress = "ul. Przykładowa 1, 00-000 Warszawa",
                    CompanyTaxID = "1234567890",
                    InvoicePrefix = "FV",
                    DefaultPaymentDays = 14,
                    InvoiceRetentionDays = 0
                };

                await _settingsStorage.SaveAsync(settings);
            }

            _cachedSettings = settings;
            return settings;
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            await _settingsStorage.SaveAsync(settings);
            _cachedSettings = settings;
        }

        public string[] GetVatRates()
        {
            return new[] { "23%", "8%", "5%", "0%" };
        }

        public string[] GetPaymentMethods()
        {
            return new[] { "Przelew", "Gotówka" };
        }

        public int[] GetInvoiceRetentionOptions()
        {
            return new[] { 0, 90, 180, 270, 360 };
        }
    }
}