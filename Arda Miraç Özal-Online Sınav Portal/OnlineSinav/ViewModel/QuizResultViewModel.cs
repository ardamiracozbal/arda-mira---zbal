using OnlineSinav.Controllers;

namespace OnlineSinav.ViewModel
{
    public class QuizResultViewModel
    {
        public string QuizTitle { get; set; }
        public double Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CompletedAt { get; set; }
        public List<QuizResultAnswerViewModel> Answers { get; set; }
    }
}
