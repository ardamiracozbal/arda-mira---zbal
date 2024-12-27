namespace OnlineSinav.Models
{
    public class Classroom
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public ICollection<AppUser> Users { get; set; } // Navigation property

    }
}
