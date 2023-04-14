using Appointment_Scheduler.Data;
using Appointment_Scheduler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System;
using System.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Threading.Channels;

namespace Appointment_Scheduler.Controllers
{
    public class AppointmentsController : Controller
    {
        readonly AppointmentDetailsDBContext db;
        IConfiguration configuration;
        public AppointmentsController(AppointmentDetailsDBContext dbContext, IConfiguration configuration)
        {
            db = dbContext;
            this.configuration = configuration;
        }
        public void updateToComplete(int appointment_id)
        {
            var item = db.AppointmentDetails.Where(p => p.appointment_id == appointment_id).FirstOrDefault();

            if (item != null)
            {
                item.status = "Completed";
                db.SaveChanges();
            }
        }
public List<AppointmentDetailsModel> getAppointments()
{
    List<AppointmentDetailsModel> appointmentList= new List<AppointmentDetailsModel>();
    string current_user_email = Request.Cookies["current_user_email"];
    string conn_string = configuration.GetConnectionString("Appointment_Scheduler");
    SqlConnection conn = new SqlConnection();
    conn.ConnectionString = conn_string;
    conn.Open();
    SqlCommand cmd = conn.CreateCommand();
    string query = $"select * from AppointmentDetails where email='{current_user_email}' order by appointment_id";
    cmd.CommandText = query;
    SqlDataReader reader = cmd.ExecuteReader();
    while (reader.Read())
    {
                AppointmentDetailsModel model = new AppointmentDetailsModel();
                model.appointment_id = (int)reader["appointment_id"];
                model.email = (string)reader["email"];
                model.start_time = (TimeSpan)reader["start_time"];
                model.end_time = (TimeSpan)reader["end_time"];
                model.duration = (string)reader["duration"];
                model.date = (DateTime)reader["date"];
                model.description = (string)reader["description"];
                string curr_status = (string)reader["status"];
                DateTime currentDate = DateTime.Now;
                if (model.date < Convert.ToDateTime(currentDate.ToShortDateString()))
                {
                    if (!curr_status.Equals("Completed"))
                    {
                        updateToComplete(model.appointment_id);
                    }
                    model.status = "Completed";
                }
                else
                {
                    if (model.start_time < TimeSpan.Parse(currentDate.ToString("HH:mm:ss")))
                    {
                        if (!curr_status.Equals("Completed"))
                        {
                            updateToComplete(model.appointment_id);
                        }
                        model.status = "Completed";
                    }
                    else
                    {
                        model.status = "Upcoming";
                    }
                }
                appointmentList.Add(model);
            }
            reader.Close();
            conn.Close();
            //appointmentList = db.AppointmentDetails.Where(x=>x.email==current_user_email).ToList();
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
        public ActionResult Create(AppointmentDetailsModel model)
        {
            DateTime currentDate = DateTime.Now;
            Console.WriteLine("Current date: " + currentDate.ToString("yyyy-MM-dd"));

            if (model.start_time > model.end_time)
            {
                Console.WriteLine("start time should be lesser than end time");
                ViewBag.Error = "Start Time Should be Lesser than the End Time";
                return View();
            }
            else if((model.date) < currentDate)
            {
                ViewBag.Error = "Scheduled date is lesser than Current date";
                Console.WriteLine(" scheduled date is lesser than current date ");
                return View();
            }

            var duration = model.end_time - model.start_time;
            //01:00:00
            string duration_string=duration.ToString();
            model.duration = duration_string.Substring(0,2)+" Hours "+ duration_string.Substring(3,2)+" Mins";
            Console.WriteLine(model.duration);
            try
            {
                string conn_string = configuration.GetConnectionString("Appointment_Scheduler");
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = conn_string;
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                string query = "INSERT INTO AppointmentDetails VALUES (@email, @start_time, @end_time, @duration, @date, @description, @status)";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@email", model.email);
                cmd.Parameters.AddWithValue("@start_time", model.start_time);
                cmd.Parameters.AddWithValue("@end_time", model.end_time);
                cmd.Parameters.AddWithValue("@duration", model.duration);
                cmd.Parameters.AddWithValue("@date", model.date);
                cmd.Parameters.AddWithValue("@description", model.description);
                cmd.Parameters.AddWithValue("@status", model.status);
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                conn.Close();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return View();
            }
        }

        public AppointmentDetailsModel getAppointmentsById(int id)
        {
            AppointmentDetailsModel admodel = new AppointmentDetailsModel();
            admodel = db.AppointmentDetails.Where(x => x.appointment_id == id).FirstOrDefault();
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
