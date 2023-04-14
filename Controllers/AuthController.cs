﻿using Appointment_Scheduler.Data;
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

        // GET: AuthController/Logout
        public ActionResult logout()
        {
            Response.Cookies.Delete("logged_in");
            return RedirectToAction("Index", "Home");

        }
    }
}
