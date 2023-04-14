﻿using Appointment_Scheduler.Data;
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

    }
}
