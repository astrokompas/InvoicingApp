using System;
using System.Collections.Generic;
using InvoicingApp.DataStorage;

namespace InvoicingApp.Models
{
    public class Client : IEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Address { get; set; }
        public string TaxID { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}