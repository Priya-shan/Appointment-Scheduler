using Appointment_Scheduler.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<UserDetailsDBContext>(options => options.UseSqlServer("Data Source=DESKTOP-KN3OCS1;Initial Catalog=Appointment_Scheduler;Integrated Security=True;Encrypt=false"));
builder.Services.AddDbContext<AppointmentDetailsDBContext>(options => options.UseSqlServer("Data Source=DESKTOP-KN3OCS1;Initial Catalog=Appointment_Scheduler;Integrated Security=True;Encrypt=false"));
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
  