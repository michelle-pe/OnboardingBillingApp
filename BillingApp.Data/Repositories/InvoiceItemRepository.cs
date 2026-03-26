using BillingApp.Data.Helpers;
using BillingApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BillingApp.Data.Repositories
{
    public class InvoiceItemRepository
    {
        private readonly string _connectionString;
        private const string QueryFile = "InvoiceItemQueries";

        public InvoiceItemRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<InvoiceItem> GetByInvoiceId(int invoiceId)
        {
            var list = new List<InvoiceItem>();
            var sql = QueryLoader.GetQuery(QueryFile, "GetItemsByInvoiceId");

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

        public void Insert(InvoiceItem item)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "InsertInvoiceItem");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", item.InvoiceId);
                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteByInvoiceId(int invoiceId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "DeleteItemsByInvoiceId");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public decimal GetInvoiceTotal(int invoiceId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "GetInvoiceTotal");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                conn.Open();
                return (decimal)cmd.ExecuteScalar();
            }
        }

        private InvoiceItem Map(IDataReader r) => new InvoiceItem
        {
            InvoiceItemId = (int)r["INVOICE_ITEM_ID"],
            InvoiceId = (int)r["INVOICE_ID"],
            ProductId = (int)r["PRODUCT_ID"],
            Quantity = (int)r["QUANTITY"],
            UnitPrice = (decimal)r["UNIT_PRICE"],
            LineTotal = (decimal)r["LINE_TOTAL"],
            ProductName = r["PRODUCT_NAME"].ToString(),
            Description = r["DESCRIPTION"] == DBNull.Value ? null : r["DESCRIPTION"].ToString()
        };
    }
}