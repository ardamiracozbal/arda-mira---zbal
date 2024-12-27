using OnlineSinav.Models;

namespace OnlineSinav.ViewModel
{
    public class TakeQuizViewModel
    {
        public int StudentQuizId { get; set; }
        public string QuizTitle { get; set; }
        public List<Question> Questions { get; set; }
        public int TotalQuestions { get; set; }
    }
}
