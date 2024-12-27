namespace OnlineSinav.Models
{
    public class Option
    {
        public int Id { get; set; }
        public string Text { get; set; } // Şık metni
        public bool IsCorrect { get; set; } // Doğru cevap olup olmadığı
        public int QuestionId { get; set; } // Hangi soruya ait olduğu
        public Question Question { get; set; } // Navigation Property
    }
}
