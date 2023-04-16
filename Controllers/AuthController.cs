using Appointment_Scheduler.Data;
using Appointment_Scheduler.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SendGrid.Helpers.Mail;
using SendGrid;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Appointment_Scheduler.Controllers
{
    public class AuthController : Controller
    {
        readonly UserDetailsDBContext db;
        readonly IHttpContextAccessor _httpContextAccessor;
        IConfiguration configuration;
        public int otp_generated = 0;
        public AuthController(UserDetailsDBContext dbContext, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            db = dbContext;
            _httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
        }

        public void sendOtp(string email)
        {
            Random rand = new Random();
            string otp = rand.Next(1234, 9999).ToString();
            //send email
            string apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            Console.WriteLine(apiKey);
            if (apiKey != null)
            {
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("shanmugapriyashanu2002@gmail.com", "Appointment Scheduler");
                var subject = "A Reminder for your Upcoming Appointment !";
                var to = new EmailAddress(email, "Example User");
                var plainTextContent = "TESTING EMAIL";
                var htmlContent = $"<strong>Your One Time Password : {otp} </strong>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = client.SendEmailAsync(msg).Result;
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(otp);
            }
            HttpContext.Response.Cookies.Append("generated_otp", otp);

        }

        public void sendmail()
        {
            Console.WriteLine("mail sent successfully");
            //generate otp

            //send mail
            int otp = 7778;
            Response.Cookies.Append("otpsent", "7777");
        }

        // GET: AuthController/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: AuthController/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(IFormCollection form)
        {
            string email = form["email"];
            string username = form["username"];
            string password = form["password"];
            string confirm_password = form["confirm_password"];
            if (password != null && !password.Equals(confirm_password))
            {
                ViewBag.Error = "Password and Confirm Password is not Matching";
                return View();
            }

            try
            {
                string conn_string = configuration.GetConnectionString("Appointment_Scheduler");
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = conn_string;
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                string query = $"Insert into UserDetails values('{email}','{username}','{password}')";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                conn.Close();
                return RedirectToAction(nameof(Login));
            }
            catch (Exception e)
            {
                ViewBag.Error = "Account Already Exists !! Try Login :)";
                Console.WriteLine(e.Message);
                return View();
            }
        }

        // GET: AuthController/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: AuthController/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserDetailsModel user)
        {

            try
            {
                string actual_pw = db.UserDetails.Where(x => x.email == user.email).Select(x => x.password).FirstOrDefault();
                if (actual_pw != null && actual_pw.Equals(user.password))
                {
                    ViewBag.Error = "";
                    HttpContext.Response.Cookies.Append("logged_in", "true");
                    HttpContext.Response.Cookies.Append("current_user_email", user.email);
                    return RedirectToAction("Index", "Appointments");
                }
                else
                {
                    ViewBag.Error = "Username or Password Incorrect";
                    return View();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return View();
            }
        }

        // GET: AuthController/Register
        public ActionResult Profile()
        {
            return View();
        }

        public ActionResult ResetPw()
        {
            Console.WriteLine("reset get");
            
            ViewBag.Error = "";
            ViewBag.otpverified= "";
            ViewBag.resetEmail = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPw(IFormCollection form)
        {
            Console.WriteLine("reset post");
            string form_name = form["formname"];
            if (form_name.Equals("mail_form"))
            {
                sendOtp(form["email"]);
                HttpContext.Response.Cookies.Append("current_user_email", form["email"]);
                ViewBag.otpverified = "no";
            }
            else if (form_name.Equals("otp_form"))
            {
                string actual_otp = Request.Cookies["generated_otp"];
                Console.WriteLine(actual_otp);
                Console.WriteLine("reseting pw");
                string recieved_otp = form["otp"];
                if (actual_otp.Equals(recieved_otp))
                {
                    ViewBag.otpverified = "yes";
                    return View();
                }
                else
                {
                    ViewBag.otpverified = "no";
                    ViewBag.Error = "Otp is Incorrect";
                    return View();
                }
            }
            else
            {
                string new_pw = form["newpw"];
                string confirm_pw = form["confirmpw"];
                var email= Request.Cookies["current_user_email"];
                if (new_pw.Equals(confirm_pw))
                {
                    Console.WriteLine("Password updated successfully");
                    var user = db.UserDetails.FirstOrDefault(a => a.email == email);
                    if (user != null)
                    {
                        user.password = new_pw;
                        db.SaveChanges();
                    }
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    ViewBag.Error = "Password and Confirm Password not matching";
                    ViewBag.otpverified = "yes";
                    return View();
                }
            }
            
            return View();
        }
        

        // GET: AuthController/Logout
        public ActionResult logout()
        {
            Response.Cookies.Delete("logged_in");
            return RedirectToAction("Index", "Home");

        }


        public ActionResult changePw()
        {
            return RedirectToAction("Login", "Auth");
        }
    }
}
