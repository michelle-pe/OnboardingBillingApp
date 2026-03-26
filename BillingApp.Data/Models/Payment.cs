using System;

namespace BillingApp.Data.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Reference { get; set; }
        public DateTime CreatedDate { get; set; }

        // Joined field
        public string InvoiceNumber { get; set; }
    }
}