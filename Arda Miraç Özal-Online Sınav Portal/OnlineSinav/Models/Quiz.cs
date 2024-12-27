namespace OnlineSinav.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; } // Quiz başlığı
        public DateTime StartDate { get; set; } // Quiz'in başlangıç tarihi
        public DateTime EndDate { get; set; } // Quiz'in bitiş tarihi
        public bool IsActive { get; set; } // Aktif/Pasif durumu
        public string TeacherId { get; set; } // Quiz'i oluşturan öğretmen
        public AppUser Teacher { get; set; } // Navigation Property
        public ICollection<Question> Questions { get; set; } // Sorular

        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }
    }
}
