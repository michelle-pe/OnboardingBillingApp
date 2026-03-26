using System;
using System.Web.Mvc;
using BillingApp.Business.Services;
using BillingApp.Data.Models;

namespace BillingApp.Web.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly CustomerService _service;

        public CustomerController()
        {
            _service = new CustomerService(ConnectionString);
        }

        // GET /Customer
        public ActionResult Index()
        {
            return View();
        }

        // AJAX — GET all customers
        [HttpGet]
        public JsonResult GetAll()
        {
            try
            {
                var customers = _service.GetAllCustomers();
                return Json(new { success = true, data = customers }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX — GET single customer
        [HttpGet]
        public JsonResult GetById(int id)
        {
            try
            {
                var customer = _service.GetCustomerById(id);
                return Json(new { success = true, data = customer }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX — CREATE
        [HttpPost]
        public JsonResult Create(Customer customer)
        {
            try
            {
                _service.CreateCustomer(customer);
                return Json(new { success = true, message = "Customer created successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX — UPDATE
        [HttpPost]
        public JsonResult Update(Customer customer)
        {
            try
            {
                _service.UpdateCustomer(customer);
                return Json(new { success = true, message = "Customer updated successfully." });
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
                _service.DeleteCustomer(id);
                return Json(new { success = true, message = "Customer deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}