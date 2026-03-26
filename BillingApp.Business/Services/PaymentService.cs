using System;
using System.Collections.Generic;
using System.Linq;
using BillingApp.Data.Models;
using BillingApp.Data.Repositories;

namespace BillingApp.Business.Services
{
    public class PaymentService
    {
        private readonly PaymentRepository _paymentRepo;
        private readonly InvoiceRepository _invoiceRepo;
        private readonly InvoiceItemRepository _itemRepo;
        private readonly InvoiceService _invoiceService;

        public PaymentService(string connectionString)
        {
            _paymentRepo = new PaymentRepository(connectionString);
            _invoiceRepo = new InvoiceRepository(connectionString);
            _itemRepo = new InvoiceItemRepository(connectionString);
            _invoiceService = new InvoiceService(connectionString);
        }

        public List<Payment> GetPaymentsByInvoice(int invoiceId)
        {
            return _paymentRepo.GetByInvoiceId(invoiceId);
        }

        public void RecordPayment(Payment payment)
        {
            ValidatePayment(payment);
            _paymentRepo.Insert(payment);

            // After every payment, recalculate and update invoice status
            _invoiceService.RecalculateStatus(payment.InvoiceId);
        }

        public void DeletePayment(int paymentId)
        {
            var payment = _paymentRepo.GetById(paymentId);
            if (payment == null)
                throw new Exception("Payment not found.");

            _paymentRepo.Delete(paymentId);

            // Recalculate invoice status now that a payment is removed
            _invoiceService.RecalculateStatus(payment.InvoiceId);
        }

        // -- Dashboard ------------------------------------------------
        public decimal GetTotalRevenue()
        {
            // Sum across all payment records
            decimal total = 0;
            var all = _paymentRepo.GetByInvoiceId(0); // we'll add a GetAll to repo
            // Simpler: call directly from controller using repo — or add GetAll()
            // For now, service exposes this for the dashboard controller
            return total;
        }

        // -- Validation -----------------------------------------------
        private void ValidatePayment(Payment payment)
        {
            if (payment.InvoiceId <= 0)
                throw new Exception("A valid invoice must be selected.");

            var invoice = _invoiceRepo.GetById(payment.InvoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found.");

            if (invoice.Status == "Cancelled")
                throw new Exception("Cannot record a payment against a cancelled invoice.");

            if (invoice.Status == "Paid")
                throw new Exception("This invoice is already fully paid.");

            if (payment.Amount <= 0)
                throw new Exception("Payment amount must be greater than zero.");

            // Business rule: payment cannot exceed remaining balance
            decimal invoiceTotal = _itemRepo.GetInvoiceTotal(payment.InvoiceId);
            decimal totalPaid = _paymentRepo.GetTotalPaid(payment.InvoiceId);
            decimal remaining = invoiceTotal - totalPaid;

            if (payment.Amount > remaining)
                throw new Exception($"Payment amount (${payment.Amount:F2}) exceeds the remaining balance (${remaining:F2}).");

            if (string.IsNullOrWhiteSpace(payment.PaymentMethod))
                throw new Exception("Payment method is required.");

            if (payment.PaymentDate == DateTime.MinValue)
                throw new Exception("Payment date is required.");
        }
    }
}