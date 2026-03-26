using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BillingApp.Data.Helpers;
using BillingApp.Data.Models;

namespace BillingApp.Data.Repositories
{
    public class InvoiceRepository
    {
        private readonly string _connectionString;
        private const string QueryFile = "InvoiceQueries";

        public InvoiceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Invoice> GetAll()
        {
            var list = new List<Invoice>();
            var sql = QueryLoader.GetQuery(QueryFile, "GetAllInvoices");

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

        public List<Invoice> GetByStatus(string status)
        {
            var list = new List<Invoice>();
            var sql = QueryLoader.GetQuery(QueryFile, "GetInvoicesByStatus");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Status", status);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(Map(reader));
            }
            return list;
        }

        public Invoice GetById(int invoiceId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "GetInvoiceById");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    return reader.Read() ? Map(reader) : null;
            }
        }

        public int Insert(Invoice invoice)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "InsertInvoice");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                cmd.Parameters.AddWithValue("@CustomerId", invoice.CustomerId);
                cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                cmd.Parameters.AddWithValue("@DueDate", invoice.DueDate);
                cmd.Parameters.AddWithValue("@Notes", (object)invoice.Notes ?? DBNull.Value);
                conn.Open();
                // The InsertInvoice query returns SCOPE_IDENTITY()
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Invoice invoice)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "UpdateInvoice");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoice.InvoiceId);
                cmd.Parameters.AddWithValue("@CustomerId", invoice.CustomerId);
                cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                cmd.Parameters.AddWithValue("@DueDate", invoice.DueDate);
                cmd.Parameters.AddWithValue("@Notes", (object)invoice.Notes ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateStatus(int invoiceId, string status)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "UpdateInvoiceStatus");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                cmd.Parameters.AddWithValue("@Status", status);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Cancel(int invoiceId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "CancelInvoice");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool HasPayments(int invoiceId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "HasPayments");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public string GetNextInvoiceNumber()
        {
            var sql = QueryLoader.GetQuery(QueryFile, "GetNextInvoiceNumber");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                return cmd.ExecuteScalar().ToString();
            }
        }

        private Invoice Map(IDataReader r) => new Invoice
        {
            InvoiceId = (int)r["INVOICE_ID"],
            InvoiceNumber = r["INVOICE_NUMBER"].ToString(),
            CustomerId = (int)r["CUSTOMER_ID"],
            InvoiceDate = (DateTime)r["INVOICE_DATE"],
            DueDate = (DateTime)r["DUE_DATE"],
            Notes = r["NOTES"] == DBNull.Value ? null : r["NOTES"].ToString(),
            Status = r["STATUS"].ToString(),
            CreatedDate = (DateTime)r["CREATED_DATE"],
            CustomerFirstName = r.HasColumn("FIRST_NAME") ? r["FIRST_NAME"].ToString() : null,
            CustomerLastName = r.HasColumn("LAST_NAME") ? r["LAST_NAME"].ToString() : null,
            CustomerEmail = r.HasColumn("EMAIL") ? r["EMAIL"].ToString() : null,
            InvoiceTotal = r.HasColumn("INVOICE_TOTAL")
                                    ? Convert.ToDecimal(r["INVOICE_TOTAL"]) : 0m
        };
    }

    // Small extension used in the mapper above
    internal static class DataReaderExtensions
    {
        public static bool HasColumn(this IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}