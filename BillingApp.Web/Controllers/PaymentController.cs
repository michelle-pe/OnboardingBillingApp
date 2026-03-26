using System;
using System.Web.Mvc;
using BillingApp.Business.Services;
using BillingApp.Data.Models;

namespace BillingApp.Web.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly PaymentService _service;

        public PaymentController()
        {
            _service = new PaymentService(ConnectionString);
        }

        // AJAX — CREATE payment
        [HttpPost]
        public JsonResult Create(Payment payment)
        {
            try
            {
                _service.RecordPayment(payment);
                return Json(new { success = true, message = "Payment recorded successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX — DELETE payment
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                _service.DeletePayment(id);
                return Json(new { success = true, message = "Payment deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}