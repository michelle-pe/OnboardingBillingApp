using System;
using System.Collections.Generic;
using BillingApp.Data.Models;
using BillingApp.Data.Repositories;

namespace BillingApp.Business.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepo;

        public ProductService(string connectionString)
        {
            _productRepo = new ProductRepository(connectionString);
        }

        public List<Product> GetAllProducts()
        {
            return _productRepo.GetAll();
        }

        public Product GetProductById(int productId)
        {
            var product = _productRepo.GetById(productId);
            if (product == null)
                throw new Exception("Product not found.");
            return product;
        }

        public void CreateProduct(Product product)
        {
            ValidateProduct(product);
            _productRepo.Insert(product);
        }

        public void UpdateProduct(Product product)
        {
            ValidateProduct(product);

            var existing = _productRepo.GetById(product.ProductId);
            if (existing == null)
                throw new Exception("Product not found.");

            _productRepo.Update(product);
        }

        public void DeleteProduct(int productId)
        {
            // Business rule: cannot delete if used in any invoice
            if (_productRepo.IsUsedInInvoice(productId))
                throw new Exception("Cannot delete product because it is used in one or more invoices.");

            _productRepo.Delete(productId);
        }

        // -- Validation --------------------------------------------------
        private void ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new Exception("Product name is required.");

            if (product.UnitPrice <= 0)
                throw new Exception("Unit price must be greater than zero.");
        }
    }
}