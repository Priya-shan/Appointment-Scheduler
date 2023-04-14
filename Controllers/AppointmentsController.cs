using Appointment_Scheduler.Data;
using Appointment_Scheduler.Models;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;

namespace Appointment_Scheduler.Controllers
{
    public class AppointmentsController : Controller
    {
        readonly AppointmentDetailsDBContext db;
        public AppointmentsController(AppointmentDetailsDBContext dbContext)
        {
            db = dbContext;
        }
        
        public List<AppointmentDetailsModel> getAppointments()
        {
            List<AppointmentDetailsModel> appointmentList= new List<AppointmentDetailsModel>();
            appointmentList = db.AppointmentDetails.ToList();
            foreach(var item in appointmentList)
            {
                Console.WriteLine("email :"+item.email);
            }
            return appointmentList;
        }
        
        // GET: AppointmentsController
        public ActionResult Index()
        {
            return View(getAppointments());
        }
        

    }
}
