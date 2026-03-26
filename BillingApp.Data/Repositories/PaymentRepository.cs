using BillingApp.Data.Helpers;
using BillingApp.Data.Models;
using BillingApp.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BillingApp.Data.Repositories
{
    public class PaymentRepository
    {
        private readonly string _connectionString;
        private const string QueryFile = "PaymentQueries";

        public PaymentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Payment> GetByInvoiceId(int invoiceId)
        {
            var list = new List<Payment>();
            var sql = QueryLoader.GetQuery(QueryFile, "GetPaymentsByInvoiceId");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(Map(reader));
            }
            return list;
        }

        public Payment GetById(int paymentId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "GetPaymentById");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    return reader.Read() ? Map(reader) : null;
            }
        }

        public void Insert(Payment payment)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "InsertPayment");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", payment.InvoiceId);
                cmd.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
                cmd.Parameters.AddWithValue("@Amount", payment.Amount);
                cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                cmd.Parameters.AddWithValue("@Reference", (object)payment.Reference ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int paymentId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "DeletePayment");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public decimal GetTotalPaid(int invoiceId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "GetTotalPaidByInvoiceId");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                conn.Open();
                return (decimal)cmd.ExecuteScalar();
            }
        }

        private Payment Map(IDataReader r) => new Payment
        {
            PaymentId = (int)r["PAYMENT_ID"],
            InvoiceId = (int)r["INVOICE_ID"],
            PaymentDate = (DateTime)r["PAYMENT_DATE"],
            Amount = (decimal)r["AMOUNT"],
            PaymentMethod = r["PAYMENT_METHOD"].ToString(),
            Reference = r["REFERENCE"] == DBNull.Value ? null : r["REFERENCE"].ToString(),
            CreatedDate = r["CREATED_DATE"] == DBNull.Value ? DateTime.MinValue : (DateTime)r["CREATED_DATE"],
            InvoiceNumber = r.HasColumn("INVOICE_NUMBER") ? r["INVOICE_NUMBER"].ToString() : null
        };
    }
}
