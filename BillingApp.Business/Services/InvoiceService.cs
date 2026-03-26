using System;
using System.Collections.Generic;
using System.Linq;
using BillingApp.Data.Models;
using BillingApp.Data.Repositories;

namespace BillingApp.Business.Services
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepo;
        private readonly InvoiceItemRepository _itemRepo;
        private readonly PaymentRepository _paymentRepo;

        public InvoiceService(string connectionString)
        {
            _invoiceRepo = new InvoiceRepository(connectionString);
            _itemRepo = new InvoiceItemRepository(connectionString);
            _paymentRepo = new PaymentRepository(connectionString);
        }

        // -- List -----------------------------------------------------
        public List<Invoice> GetAllInvoices()
        {
            return _invoiceRepo.GetAll();
        }

        public List<Invoice> GetInvoicesByStatus(string status)
        {
            ValidateStatus(status);
            return _invoiceRepo.GetByStatus(status);
        }

        // -- Detail ---------------------------------------------------
        public Invoice GetInvoiceById(int invoiceId)
        {
            var invoice = _invoiceRepo.GetById(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found.");

            // Attach line items and payment history
            invoice.Items = _itemRepo.GetByInvoiceId(invoiceId);
            invoice.Payments = _paymentRepo.GetByInvoiceId(invoiceId);

            return invoice;
        }

        // -- Create ---------------------------------------------------
        public void CreateInvoice(Invoice invoice)
        {
            ValidateInvoice(invoice);

            // Auto-generate invoice number e.g. INV-0005
            invoice.InvoiceNumber = _invoiceRepo.GetNextInvoiceNumber();

            // Insert header and get the new ID back
            int newInvoiceId = _invoiceRepo.Insert(invoice);

            // Insert each line item against the new invoice
            foreach (var item in invoice.Items)
            {
                item.InvoiceId = newInvoiceId;
                _itemRepo.Insert(item);
            }
        }

        // -- Update (Pending only) -------------------------------------
        public void UpdateInvoice(Invoice invoice)
        {
            var existing = _invoiceRepo.GetById(invoice.InvoiceId);
            if (existing == null)
                throw new Exception("Invoice not found.");

            if (existing.Status != "Pending")
                throw new Exception("Only Pending invoices can be edited.");

            ValidateInvoice(invoice);

            // Replace header
            _invoiceRepo.Update(invoice);

            // Replace all line items (delete + re-insert is simplest approach)
            _itemRepo.DeleteByInvoiceId(invoice.InvoiceId);
            foreach (var item in invoice.Items)
            {
                item.InvoiceId = invoice.InvoiceId;
                _itemRepo.Insert(item);
            }
        }

        // -- Cancel ---------------------------------------------------
        public void CancelInvoice(int invoiceId)
        {
            var existing = _invoiceRepo.GetById(invoiceId);
            if (existing == null)
                throw new Exception("Invoice not found.");

            // Business rule: can only cancel if no payments exist
            if (_invoiceRepo.HasPayments(invoiceId))
                throw new Exception("Cannot cancel invoice because payments have been recorded against it.");

            _invoiceRepo.Cancel(invoiceId);
        }

        // -- Called by PaymentService after every payment change ------
        public void RecalculateStatus(int invoiceId)
        {
            decimal invoiceTotal = _itemRepo.GetInvoiceTotal(invoiceId);
            decimal totalPaid = _paymentRepo.GetTotalPaid(invoiceId);

            string newStatus;

            if (totalPaid >= invoiceTotal)
                newStatus = "Paid";
            else if (DateTime.Now > _invoiceRepo.GetById(invoiceId).DueDate)
                newStatus = "Overdue";
            else
                newStatus = "Pending";

            _invoiceRepo.UpdateStatus(invoiceId, newStatus);
        }

        // -- Dashboard ------------------------------------------------
        public List<Invoice> GetRecentInvoices()
        {
            return _invoiceRepo.GetAll();   // Already ordered DESC; take top 5 in view
        }

        // -- Validation -----------------------------------------------
        private void ValidateInvoice(Invoice invoice)
        {
            if (invoice.CustomerId <= 0)
                throw new Exception("A customer must be selected.");

            if (invoice.InvoiceDate == DateTime.MinValue)
                throw new Exception("Invoice date is required.");

            if (invoice.DueDate == DateTime.MinValue)
                throw new Exception("Due date is required.");

            if (invoice.DueDate < invoice.InvoiceDate)
                throw new Exception("Due date cannot be earlier than the invoice date.");

            if (invoice.Items == null || invoice.Items.Count == 0)
                throw new Exception("An invoice must have at least one line item.");

            foreach (var item in invoice.Items)
            {
                if (item.ProductId <= 0)
                    throw new Exception("Each line item must have a product selected.");

                if (item.Quantity < 1)
                    throw new Exception("Quantity must be at least 1.");

                if (item.UnitPrice <= 0)
                    throw new Exception("Unit price must be greater than zero.");
            }
        }

        private void ValidateStatus(string status)
        {
            var valid = new[] { "Pending", "Paid", "Overdue", "Cancelled" };
            if (!Array.Exists(valid, s => s == status))
                throw new Exception($"Invalid status: {status}");
        }
    }
}