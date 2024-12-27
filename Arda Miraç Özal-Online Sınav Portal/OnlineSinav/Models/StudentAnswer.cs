namespace OnlineSinav.Models
{
    public class StudentAnswer
    {
        public int Id { get; set; }
        public int StudentQuizId { get; set; } // Bu özellik doðru bir þekilde tanýmlanmalý
        public StudentQuiz StudentQuiz { get; set; } // Navigation Property
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public int SelectedOptionId { get; set; }
        public Option SelectedOption { get; set; }
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
} 