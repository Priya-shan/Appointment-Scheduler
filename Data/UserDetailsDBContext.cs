using Microsoft.EntityFrameworkCore;
using Appointment_Scheduler.Models;

namespace Appointment_Scheduler.Data
{
    public class UserDetailsDBContext : DbContext
    {
        public UserDetailsDBContext(DbContextOptions<UserDetailsDBContext> options) : base(options)
        {
        }

        public DbSet<UserDetailsModel> UserDetails { get; set; }
    }
}
