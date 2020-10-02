using Microsoft.AspNetCore.Mvc;

namespace FiverrNotifications.Controllers
{
    [Route("api")]
    public class MaintananceController : Controller
    {
        [Route("ping")]
        public IActionResult Ping()
        {
            return Content("OK", new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/plain"));
        }
    }
}