using MessagePack;
using Microsoft.Build.Framework;

using System.ComponentModel.DataAnnotations;
namespace Appointment_Scheduler.Models
{
    public class UserDetailsModel
    {
        //public int userid { get; set; }
        [System.ComponentModel.DataAnnotations.Key]
        [System.ComponentModel.DataAnnotations.Required]
        public string email { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string username { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string password { get; set; }

    }

}
