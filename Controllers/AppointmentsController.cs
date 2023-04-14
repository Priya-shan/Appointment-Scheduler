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

        // GET: AppointmentsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AppointmentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("email", "start_time", "end_time", "duration", "date", "description", "status")] AppointmentDetailsModel AppDet)
        {

            try
            {
                db.Add(AppDet);
                await db.SaveChangesAsync();
                Console.WriteLine("added to db");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return View();
            }
        }

        public AppointmentDetailsModel getAppointmentsById(int id)
        {
            AppointmentDetailsModel admodel = new AppointmentDetailsModel();
            admodel = db.AppointmentDetails.Where(x => x.appointment_id == id).FirstOrDefault();
            Console.WriteLine(admodel.appointment_id);
            Console.WriteLine(admodel.email);
            //foreach (var item in datas)
            //{
            //    Console.WriteLine("email :" + item.email);
            //}
            return admodel;
        }

        // GET: AppointmentsController/Edit/5
        public ActionResult Edit(int id)
        {

            return View(getAppointmentsById(id));
        }

        // POST: AppointmentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("appointment_id", "email", "start_time", "end_time", "duration", "date", "description", "status")] AppointmentDetailsModel AppDet1)
        {
            try
            {
                Console.WriteLine("gonna update");
                db.Update(AppDet1);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AppointmentsController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var data_to_delete = await db.AppointmentDetails.FindAsync(id);
            db.Remove(data_to_delete);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



    }
}
