using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QRFileTrackingapi.Models.Entities
{
    public class UserAccount : IdentityUser
    {
        [Required]
        public string EpfNo { get; set; }

        [Required]
        public string Name { get; set; }

        public string Phone { get; set; }
        public string Department { get; set; }
        public string SeatNo { get; set; }
        public string Role { get; set; }
        public bool IsApproved { get; set; }
    }
}
