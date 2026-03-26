using System;
using System.Web.Mvc;
using BillingApp.Business.Services;

namespace BillingApp.Web.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly DashboardService _service;

        public DashboardController()
        {
            _service = new DashboardService(ConnectionString);
        }

        // GET /Dashboard  (home page)
        public ActionResult Index()
        {
            return View();
        }

        // AJAX — all stats in one call
        [HttpGet]
        public JsonResult GetStats()
        {
            try
            {
                var statusCounts = _service.GetInvoiceCountByStatus();
                var recentInvoices = _service.GetRecentInvoices();
                var recentPayments = _service.GetRecentPayments();

                var result = new
                {
                    InvoiceCounts = new
                    {
                        Pending = statusCounts.ContainsKey("Pending") ? statusCounts["Pending"] : 0,
                        Paid = statusCounts.ContainsKey("Paid") ? statusCounts["Paid"] : 0,
                        Overdue = statusCounts.ContainsKey("Overdue") ? statusCounts["Overdue"] : 0,
                        Cancelled = statusCounts.ContainsKey("Cancelled") ? statusCounts["Cancelled"] : 0
                    },
                    TotalRevenue = _service.GetTotalRevenue(),
                    TotalOutstanding = _service.GetTotalOutstanding(),
                    RecentInvoices = recentInvoices,
                    RecentPayments = recentPayments
                };

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}