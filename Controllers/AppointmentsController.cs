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
using System.Data;
using Microsoft.Extensions.Configuration;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Appointment_Scheduler.Controllers
{
    public class AppointmentsController : Controller
    {
        readonly AppointmentDetailsDBContext db;
        IConfiguration configuration;
        public SqlConnection conn;
        public AppointmentsController(AppointmentDetailsDBContext dbContext, IConfiguration configuration)
        {
            db = dbContext;
            this.configuration = configuration;
            conn = new SqlConnection(configuration.GetConnectionString("Appointment_Scheduler"));
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

            List<AppointmentDetailsModel> appointmentList = new List<AppointmentDetailsModel>();
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
                Console.WriteLine("stts : " + curr_status);
                DateTime currentDate = DateTime.Now;
                model.status = curr_status;
                Console.WriteLine("curr__date : " + Convert.ToDateTime(currentDate.ToShortDateString()));
                Console.WriteLine("app__date : " + model.date);
                if (model.date < Convert.ToDateTime(currentDate.ToShortDateString()))
                {
                    if (!curr_status.Equals("Completed"))
                    {
                        Console.WriteLine("entered 1st if");
                        updateToComplete(model.appointment_id);
                    }
                    
                }
                else if(model.date== Convert.ToDateTime(currentDate.ToShortDateString()))
                {
                    if (model.start_time < TimeSpan.Parse(currentDate.ToString("HH:mm:ss")))
                    {
                        if (!curr_status.Equals("Completed"))
                        {
                            Console.WriteLine("entered 2nd if");
                            updateToComplete(model.appointment_id);
                        }
                    }
                }
                appointmentList.Add(model);
            }
            reader.Close();
            conn.Close();
            //appointmentList = db.AppointmentDetails.Where(x=>x.email==current_user_email).ToList();
            return appointmentList;
        }

        public AppointmentDetailsModel getAppointmentsById(int id)
        {
            AppointmentDetailsModel admodel = new AppointmentDetailsModel();
            admodel = db.AppointmentDetails.Where(x => x.appointment_id == id).FirstOrDefault();
            return admodel;
        }
        // GET: AppointmentsController

        public List<AppointmentDetailsModel> getAppointmentsByFilter(string procedure_name,string keyword,DateTime start_date,DateTime end_date,string status)
        {
            string current_user= Request.Cookies["current_user_email"];
            List<AppointmentDetailsModel> appointmentList = new List<AppointmentDetailsModel>();
            conn.Open();
            SqlCommand cmd = new SqlCommand(procedure_name, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            if (procedure_name.Equals("fetch_appointments_with_name")){
                cmd.Parameters.AddWithValue("@description", keyword);
                cmd.Parameters.AddWithValue("@email", current_user);
            }
            else if (procedure_name.Equals("fetch_appointments_with_date_range"))
            {
                cmd.Parameters.AddWithValue("@start_date", start_date);
                cmd.Parameters.AddWithValue("@end_date", end_date);
                cmd.Parameters.AddWithValue("@email", current_user);
            }
            else
            {
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@email", current_user);
            }
            
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
                model.status = (string)reader["status"];
                appointmentList.Add(model);
            }
            reader.Close();
            conn.Close();
            return appointmentList;
        }
        public ActionResult Index()
        {
            return View(getAppointments());
        }

        // POST: AppointmentsController
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(IFormCollection form)
        {
            
            string form_name = form["form_name"];
            Console.WriteLine("form name : " + form_name);
            DateTime dummy = DateTime.Now;
            if (form_name != null)
            {
                if (form_name.Equals("name_search"))
                {
                    string keyword = form["name"];
                    Console.WriteLine(" name : " + keyword);

                   
                    return View(getAppointmentsByFilter("fetch_appointments_with_name",keyword,dummy,dummy,""));
                }
                else if (form_name.Equals("date_Search"))
                {
                    DateTime start_date = Convert.ToDateTime(form["start_date"]);
                    DateTime end_date = Convert.ToDateTime(form["end_date"]);
                    if (start_date > end_date)
                    {
                        ViewBag.Error = "Start Date should be letter than End Date";
                        return View(getAppointments());
                    }
                    
                    Console.WriteLine("sd :"+start_date);
                    Console.WriteLine("ed : "+end_date);

                    return View(getAppointmentsByFilter("fetch_appointments_with_date_range", "", start_date, end_date, ""));
                }
                else
                {
                    string status = form["status"];
                    Console.WriteLine("status : " + status);
                    return View(getAppointmentsByFilter("fetch_appointments_with_status", "", dummy, dummy, status));
                }
            }
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
            Console.WriteLine(currentDate);
            Console.WriteLine((model.date).Add(model.start_time));
            if (model.start_time > model.end_time)
            {
                Console.WriteLine("start time should be lesser than end time");
                ViewBag.Error = "Start Time Should be Lesser than the End Time";
                return View();
            }
            
            else if ((model.date.Add(model.start_time)) < currentDate)
            {
                ViewBag.Error = "Scheduled date is lesser than Current date";
                Console.WriteLine(" scheduled date is lesser than current date ");
                return View();
            }

            var duration = model.end_time - model.start_time;
            //01:00:00
            model.status = "Upcoming";
            string duration_string = duration.ToString();
            model.duration = duration_string.Substring(0, 2) + " Hours " + duration_string.Substring(3, 2) + " Mins";
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
                Console.WriteLine("adding status : " + model.status);
                cmd.Parameters.AddWithValue("@status", model.status);
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                conn.Close();
                int year = model.date.Year;
                int month = model.date.Month;
                int date = model.date.Day;

                int hour = model.start_time.Hours;
                int minute = model.end_time.Minutes;

                Console.WriteLine("fetching details : " + year + " " + month + " " + date + " " + hour + " " + minute);
                //SCHEDULING EMAIL ON SPECIFIED DATE TIME
                string apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
                Console.WriteLine(apiKey);
                if (apiKey != null)
                {
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress("shanmugapriyashanu2002@gmail.com", "Appointment Scheduler");
                    var subject = "A Reminder for your Upcoming Appointment !";
                    var to = new EmailAddress(model.email, "Example User");
                    var plainTextContent = "TESTING EMAIL";
                    var htmlContent = "<strong>! You have an Appointment in 30 Minutes !</strong>" +
                        "<h3>Appointment Details</h3>" +
                        $"<h5>Description : {model.description}</h5>" +
                        $"<h5>Date : {model.date.ToShortDateString()}</h5>" +
                        $"<h5>Time : {model.start_time} to {model.end_time}</h5>" +
                        $"<h5>Duration : {model.duration}</h5>";
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    // todo set batch id here
                    //msg.BatchId = "20002";
                    //msg.BatchId=Guid.NewGuid().ToString();
                    //Console.WriteLine(msg.BatchId);
                    DateTime sendDateTime = new DateTime(year,month,date,hour,minute, 0, DateTimeKind.Local);
                    sendDateTime = sendDateTime.AddMinutes(-30);
                    Console.WriteLine("send date : " + sendDateTime);
                    msg.SendAt = new DateTimeOffset(sendDateTime).ToUnixTimeSeconds();
                    var response = client.SendEmailAsync(msg).Result;
                    Console.WriteLine(response.StatusCode);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return View();
            }
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
                //todo cancel scheduled mail 
                //todo schedule correct mail
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
