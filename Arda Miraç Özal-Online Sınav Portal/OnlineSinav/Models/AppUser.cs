using Microsoft.AspNetCore.Identity;

namespace OnlineSinav.Models
{
    public class AppUser: IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsApproved { get; set; } // Admin onayı
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string TCNo { get; set; }

        public int? ClassroomId { get; set; } = 1;  
        public Classroom Classroom { get; set; }
    }
}
