using System;
using System.Collections.Generic;
using System.Web.Mvc;
using BillingApp.Business.Services;
using BillingApp.Data.Models;

namespace BillingApp.Web.Controllers
{
    public class InvoiceController : BaseController
    {
        private readonly InvoiceService _invoiceService;
        private readonly CustomerService _customerService;
        private readonly ProductService _productService;

        public InvoiceController()
        {
            _invoiceService = new InvoiceService(ConnectionString);
            _customerService = new CustomerService(ConnectionString);
            _productService = new ProductService(ConnectionString);
        }

        // GET /Invoice
        public ActionResult Index()
        {
            return View();
        }

        // GET /Invoice/Create
        public ActionResult Create()
        {
            return View();
        }

        // GET /Invoice/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.InvoiceId = id;
            return View();
        }

        // GET /Invoice/Detail/5
        public ActionResult Detail(int id)
        {
            ViewBag.InvoiceId = id;
            return View();
        }

        // -- AJAX endpoints --------------------------------------------

        [HttpGet]
        public JsonResult GetAll()
        {
            try
            {
                var invoices = _invoiceService.GetAllInvoices();
                return Json(new { success = true, data = invoices }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetById(int id)
        {
            try
            {
                var invoice = _invoiceService.GetInvoiceById(id);
                return Json(new { success = true, data = invoice }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetCustomers()
        {
            try
            {
                var customers = _customerService.GetAllCustomers();
                return Json(new { success = true, data = customers }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetProducts()
        {
            try
            {
                var products = _productService.GetAllProducts();
                return Json(new { success = true, data = products }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Create(Invoice invoice, List<InvoiceItem> items)
        {
            try
            {
                invoice.Items = items ?? new List<InvoiceItem>();
                _invoiceService.CreateInvoice(invoice);
                return Json(new { success = true, message = "Invoice created successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Update(Invoice invoice, List<InvoiceItem> items)
        {
            try
            {
                invoice.Items = items ?? new List<InvoiceItem>();
                _invoiceService.UpdateInvoice(invoice);
                return Json(new { success = true, message = "Invoice updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Cancel(int id)
        {
            try
            {
                _invoiceService.CancelInvoice(id);
                return Json(new { success = true, message = "Invoice cancelled successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}