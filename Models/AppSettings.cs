using System.Collections.Generic;
using InvoicingApp.DataStorage;

namespace InvoicingApp.Models
{
    public class AppSettings : IEntity
    {
        public string Id { get; set; } = "settings";

        // Company information
        public string CompanyName { get; set; } = "Moja Firma";
        public string CompanyAddress { get; set; } = "ul. Przykładowa 1, 00-000 Warszawa";
        public string CompanyTaxID { get; set; } = "0000000000";
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyLogoPath { get; set; }
        public string CompanyBankAccount { get; set; }
        public string CompanyContactPerson { get; set; }

        // Invoice settings
        public List<string> VatRates { get; set; } = new List<string> { "23%", "8%", "5%", "0%" };
        public string DefaultVatRate { get; set; } = "23%";
        public List<string> PaymentMethods { get; set; } = new List<string> { "Przelew", "Gotówka" };
        public string DefaultPaymentMethod { get; set; } = "Przelew";
        public int DefaultPaymentDays { get; set; } = 14;
        public List<string> Currencies { get; set; } = new List<string> { "PLN", "EUR" };
        public string Currency { get; set; } = "PLN";

        // Invoice numbering
        public string InvoicePrefix { get; set; } = "FV";
        public bool ResetNumberingYearly { get; set; } = true;

        // Data retention
        public int InvoiceRetentionDays { get; set; } = 0;
        public class RetentionOption
        {
            public int Days { get; set; }
            public string DisplayName { get; set; }
        }
    }
}