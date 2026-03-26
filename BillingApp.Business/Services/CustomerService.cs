using System;
using System.Collections.Generic;
using BillingApp.Data.Models;
using BillingApp.Data.Repositories;

namespace BillingApp.Business.Services
{
    public class CustomerService
    {
        private readonly CustomerRepository _customerRepo;

        public CustomerService(string connectionString)
        {
            _customerRepo = new CustomerRepository(connectionString);
        }

        public List<Customer> GetAllCustomers()
        {
            return _customerRepo.GetAll();
        }

        public Customer GetCustomerById(int customerId)
        {
            var customer = _customerRepo.GetById(customerId);
            if (customer == null)
                throw new Exception("Customer not found.");
            return customer;
        }

        public void CreateCustomer(Customer customer)
        {
            ValidateCustomer(customer);
            _customerRepo.Insert(customer);
        }

        public void UpdateCustomer(Customer customer)
        {
            ValidateCustomer(customer);

            var existing = _customerRepo.GetById(customer.CustomerId);
            if (existing == null)
                throw new Exception("Customer not found.");

            _customerRepo.Update(customer);
        }

        public void DeleteCustomer(int customerId)
        {
            // Business rule: cannot delete if customer has invoices
            if (_customerRepo.HasInvoices(customerId))
                throw new Exception("Cannot delete customer because they have existing invoices.");

            _customerRepo.Delete(customerId);
        }

        // -- Validation -----------------------------------------
        private void ValidateCustomer(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.FirstName))
                throw new Exception("First name is required.");

            if (string.IsNullOrWhiteSpace(customer.LastName))
                throw new Exception("Last name is required.");

            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new Exception("Email is required.");

            if (!IsValidEmail(customer.Email))
                throw new Exception("Email format is invalid.");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}