using Microsoft.CodeAnalysis.Options;

namespace OnlineSinav.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } // Soru metni
        public ICollection<Option> Options { get; set; } // Şıklar
        public int QuizId { get; set; } // Hangi quiz'e ait olduğu
        public Quiz Quiz { get; set; } // Navigation Property
    }
}
