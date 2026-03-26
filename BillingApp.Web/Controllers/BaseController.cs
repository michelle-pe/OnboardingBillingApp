using System.Configuration;
using System.Web.Mvc;

namespace BillingApp.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["BillingAppDB"].ConnectionString;
    }
}