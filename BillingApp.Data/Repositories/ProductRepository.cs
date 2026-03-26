using BillingApp.Data.Helpers;
using BillingApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BillingApp.Data.Repositories
{
    public class ProductRepository
    {
        private readonly string _connectionString;
        private const string QueryFile = "ProductQueries";

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Product> GetAll()
        {
            var list = new List<Product>();
            var sql = QueryLoader.GetQuery(QueryFile, "GetAllProducts");

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

        public Product GetById(int productId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "GetProductById");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ProductId", productId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    return reader.Read() ? Map(reader) : null;
            }
        }

        public void Insert(Product product)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "InsertProduct");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                cmd.Parameters.AddWithValue("@Description", (object)product.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(Product product)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "UpdateProduct");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                cmd.Parameters.AddWithValue("@Description", (object)product.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int productId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "DeleteProduct");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ProductId", productId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsUsedInInvoice(int productId)
        {
            var sql = QueryLoader.GetQuery(QueryFile, "IsProductUsedInInvoice");

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ProductId", productId);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private Product Map(IDataReader r) => new Product
        {
            ProductId = (int)r["PRODUCT_ID"],
            ProductName = r["PRODUCT_NAME"].ToString(),
            Description = r["DESCRIPTION"] == DBNull.Value ? null : r["DESCRIPTION"].ToString(),
            UnitPrice = (decimal)r["UNIT_PRICE"],
            IsActive = (bool)r["IS_ACTIVE"]
        };
    }
}