using System;
using System.Web.Mvc;
using BillingApp.Business.Services;
using BillingApp.Data.Models;

namespace BillingApp.Web.Controllers
{
    public class ProductController : BaseController
    {
        private readonly ProductService _service;

        public ProductController()
        {
            _service = new ProductService(ConnectionString);
        }

        // GET /Product
        public ActionResult Index()
        {
            return View();
        }

        // AJAX — GET all products
        [HttpGet]
        public JsonResult GetAll()
        {
            try
            {
                var products = _service.GetAllProducts();
                return Json(new { success = true, data = products }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX — GET single product
        [HttpGet]
        public JsonResult GetById(int id)
        {
            try
            {
                var product = _service.GetProductById(id);
                return Json(new { success = true, data = product }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX — CREATE
        [HttpPost]
        public JsonResult Create(Product product)
        {
            try
            {
                _service.CreateProduct(product);
                return Json(new { success = true, message = "Product created successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX — UPDATE
        [HttpPost]
        public JsonResult Update(Product product)
        {
            try
            {
                _service.UpdateProduct(product);
                return Json(new { success = true, message = "Product updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX — DELETE
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                _service.DeleteProduct(id);
                return Json(new { success = true, message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}