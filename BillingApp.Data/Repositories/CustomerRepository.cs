using BillingApp.Data.Helpers;
using BillingApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BillingApp.Data.Repositories
{
    public class CustomerRepository
    {
        private readonly string _connectionString;
        private const string QueryFile = "CustomerQueries";

        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Customer> GetAll()
        {
            var list = new List<Customer>();
            var sql = QueryLoader.GetQuery(QueryFile, "GetAllCustomers");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(Map(reader));
            }
            return list;
        }

        public Customer GetById(int customerId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "GetCustomerById");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    return reader.Read() ? Map(reader) : null;
            }
        }

        public void Insert(Customer customer)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "InsertCustomer");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@FirstName", customer.FirstName);
                cmd.Parameters.AddWithValue("@LastName", customer.LastName);
                cmd.Parameters.AddWithValue("@Email", customer.Email);
                cmd.Parameters.AddWithValue("@Phone", (object)customer.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)customer.Address ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(Customer customer)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "UpdateCustomer");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                cmd.Parameters.AddWithValue("@FirstName", customer.FirstName);
                cmd.Parameters.AddWithValue("@LastName", customer.LastName);
                cmd.Parameters.AddWithValue("@Email", customer.Email);
                cmd.Parameters.AddWithValue("@Phone", (object)customer.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)customer.Address ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int customerId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "DeleteCustomer");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool HasInvoices(int customerId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "HasInvoices");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        // -- private mapper ---------------------------------------------
        private Customer Map(IDataReader r) => new Customer
        {
            CustomerId = (int)r["CUSTOMER_ID"],
            FirstName = r["FIRST_NAME"].ToString(),
            LastName = r["LAST_NAME"].ToString(),
            Email = r["EMAIL"].ToString(),
            Phone = r["PHONE"] == DBNull.Value ? null : r["PHONE"].ToString(),
            Address = r["ADDRESS"] == DBNull.Value ? null : r["ADDRESS"].ToString(),
            CreatedDate = (System.DateTime)r["CREATED_DATE"],
            IsActive = (bool)r["IS_ACTIVE"]
        };
    }
}