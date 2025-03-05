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
        public string PaidRatio => $"{PaidInvoices}/{TotalInvoices}";
        public string PaymentPercentage => TotalInvoices > 0
            ? $"{(decimal)PaidInvoices / TotalInvoices * 100:0.0}%"
            : "0%";
    }
}