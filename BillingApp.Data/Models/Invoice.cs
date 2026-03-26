using System;
using System.Collections.Generic;

namespace BillingApp.Data.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public int CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        // Joined fields (populated from queries that JOIN customers)
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerFullName => CustomerFirstName + " " + CustomerLastName;

        // Calculated from invoice items
        public decimal InvoiceTotal { get; set; }

        // Navigation — populated by service layer, not DB directly
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public List<Payment> Payments { get; set; } = new List<Payment>();
    }
}