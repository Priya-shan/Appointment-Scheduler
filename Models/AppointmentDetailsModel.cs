using System.ComponentModel.DataAnnotations;

namespace Appointment_Scheduler.Models
{
    public class AppointmentDetailsModel
    {
        [Key]
        [Required]
        public int appointment_id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public TimeSpan start_time { get; set; }
        [Required]
        public TimeSpan end_time { get; set; }
        [Required]
        public string duration { get; set; }
        [Required]
        public DateTime date { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public string status { get; set; }
    }
}
