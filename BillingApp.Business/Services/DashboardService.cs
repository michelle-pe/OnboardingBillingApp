using BillingApp.Business.Services;
using BillingApp.Data.Models;
using BillingApp.Data.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace BillingApp.Business.Services
{
    public class DashboardService
    {
        private readonly InvoiceRepository _invoiceRepo;
        private readonly InvoiceItemRepository _itemRepo;
        private readonly PaymentRepository _paymentRepo;
        private readonly CustomerRepository _customerRepo;

        public DashboardService(string connectionString)
        {
            _invoiceRepo = new InvoiceRepository(connectionString);
            _itemRepo = new InvoiceItemRepository(connectionString);
            _paymentRepo = new PaymentRepository(connectionString);
            _customerRepo = new CustomerRepository(connectionString);
        }

        // Total invoices grouped by status  { "Pending": 4, "Paid": 2, ... }
        public Dictionary<string, int> GetInvoiceCountByStatus()
        {
            var result = new Dictionary<string, int>
            {
                { "Pending",   0 },
                { "Paid",      0 },
                { "Overdue",   0 },
                { "Cancelled", 0 }
            };

            foreach (var inv in _invoiceRepo.GetAll())
            {
                if (result.ContainsKey(inv.Status))
                    result[inv.Status]++;
            }

            return result;
        }

        // Sum of all payments ever recorded
        public decimal GetTotalRevenue()
        {
            decimal total = 0;
            // Re-use per-invoice method across all invoices
            foreach (var inv in _invoiceRepo.GetAll())
                total += _paymentRepo.GetTotalPaid(inv.InvoiceId);
            return total;
        }

        // Sum of unpaid balances on non-cancelled invoices
        public decimal GetTotalOutstanding()
        {
            decimal outstanding = 0;
            foreach (var inv in _invoiceRepo.GetAll())
            {
                if (inv.Status == "Cancelled") continue;
                decimal total = _itemRepo.GetInvoiceTotal(inv.InvoiceId);
                decimal paid = _paymentRepo.GetTotalPaid(inv.InvoiceId);
                outstanding += (total - paid);
            }
            return outstanding;
        }

        // 5 most recent invoices
        public List<Invoice> GetRecentInvoices()
        {
            var all = _invoiceRepo.GetAll();   // already ordered DESC
            return all.Count > 5 ? all.GetRange(0, 5) : all;
        }

        // 5 most recent payments
        public List<Payment> GetRecentPayments()
        {
            // Collect from all invoices and sort
            var all = new List<Payment>();
            foreach (var inv in _invoiceRepo.GetAll())
                all.AddRange(_paymentRepo.GetByInvoiceId(inv.InvoiceId));

            all.Sort((a, b) => b.PaymentDate.CompareTo(a.PaymentDate));
            return all.Count > 5 ? all.GetRange(0, 5) : all;
        }
    }
}
