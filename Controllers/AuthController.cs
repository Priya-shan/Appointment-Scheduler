using Appointment_Scheduler.Data;
using Appointment_Scheduler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Appointment_Scheduler.Controllers
{
    public class AuthController : Controller
    {
        readonly UserDetailsDBContext db;
        readonly IHttpContextAccessor _httpContextAccessor;
        IConfiguration configuration;
        public AuthController(UserDetailsDBContext dbContext, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            db = dbContext;
            _httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
        }

        // GET: AuthController/Create
        public ActionResult Register()
        {
            return View();
        }

        // POST: AuthController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(IFormCollection form)
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserDetailsModel user)
        {
            return View();
        }
    }
}
