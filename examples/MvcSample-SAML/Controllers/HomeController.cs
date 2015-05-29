using System.Security.Claims;
using System.Web.Mvc;

namespace MvcSampleSAML.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Claims()
        {
            return View(ClaimsPrincipal.Current.Claims);
        }
    }
}