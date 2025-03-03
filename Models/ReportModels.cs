using System;
using System.Collections.Generic;

namespace InvoicingApp.Models
{
    public class ReportSummary
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ClientId { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public int PaidInvoices { get; set; }
        public int UnpaidInvoices { get; set; }
        public List<ClientReportSummary> ClientBreakdown { get; set; } = new List<ClientReportSummary>();
    }

    public class ClientReportSummary
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public int PaidInvoices { get; set; }
        public int UnpaidInvoices { get; set; }
    }

    // This is the class you were using in your original code
    public class ReportClientBreakdown
    {
        public string Client { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalNet { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalGross { get; set; }
    }
}