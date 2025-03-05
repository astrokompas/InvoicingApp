using System;
using InvoicingApp.Models;

namespace InvoicingApp.Models
{
    public class InvoiceListItem
    {
        public string Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalGross { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => TotalGross - PaidAmount;

        // Formatted properties for display in the UI
        public string FormattedDate => InvoiceDate.ToString("dd.MM.yyyy");
        public string FormattedDueDate => DueDate.ToString("dd.MM.yyyy");
        public string FormattedAmount => $"{TotalGross:N2} PLN";

        // Status display name
        public string Status => PaymentStatus.ToString();

        // Shorthand properties
        public bool IsPaid => PaymentStatus == PaymentStatus.Paid;
        public bool IsOverdue => PaymentStatus == PaymentStatus.Overdue;
    }
}