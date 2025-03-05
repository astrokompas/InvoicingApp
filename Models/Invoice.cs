using System;
using System.Collections.Generic;
using InvoicingApp.DataStorage;

namespace InvoicingApp.Models
{
    public class Invoice : IEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public DateTime SellingDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);

        // Client relationship
        public string ClientId { get; set; }
        public Client Client { get; set; }

        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public decimal TotalNet { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalGross { get; set; }
        public string PaymentMethod { get; set; } = "Przelew";
        public string BankAccount { get; set; }
        public string Notes { get; set; }
        public bool IsPaid => PaymentStatus == PaymentStatus.Paid;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public decimal PaidAmount => Payments?.Sum(p => p.Amount) ?? 0;
        public decimal RemainingAmount => TotalGross - PaidAmount;
        public DateTime? PaymentDate => PaymentStatus == PaymentStatus.Paid ?
            Payments.OrderByDescending(p => p.Date).FirstOrDefault()?.Date : null;
    }

    public class Payment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Amount { get; set; }
        public string Method { get; set; } = "Przelew";
        public string Notes { get; set; }
    }

    public enum PaymentStatus
    {
        Unpaid,
        PartiallyPaid,
        Paid,
        Overdue
    }

    public class InvoiceItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; }
        public decimal Quantity { get; set; } = 1;
        public decimal NetPrice { get; set; }
        public string VatRate { get; set; } = "23%";
        public decimal TotalNet { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalGross { get; set; }

        // Parent invoice reference
        public string InvoiceId { get; set; }
    }
}