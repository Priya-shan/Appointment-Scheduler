using Microsoft.EntityFrameworkCore;
using Appointment_Scheduler.Models;

namespace Appointment_Scheduler.Data
{
    public class AppointmentDetailsDBContext : DbContext
    {
        public AppointmentDetailsDBContext(DbContextOptions<AppointmentDetailsDBContext> options) : base(options)
        {
        }

        public DbSet<AppointmentDetailsModel> AppointmentDetails { get; set; }
    }
}
