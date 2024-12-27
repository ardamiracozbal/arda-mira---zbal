namespace OnlineSinav.Models
{
    public class StudentQuiz
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public AppUser Student { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public double Score { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public ICollection<StudentAnswer> Answers { get; set; } = new List<StudentAnswer>();
    }
}
