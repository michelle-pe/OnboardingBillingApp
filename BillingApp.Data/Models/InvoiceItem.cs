namespace BillingApp.Data.Models
{
    public class InvoiceItem
    {
        public int InvoiceItemId { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }   // computed column from DB

        // Joined from Products
        public string ProductName { get; set; }
        public string Description { get; set; }
    }
}