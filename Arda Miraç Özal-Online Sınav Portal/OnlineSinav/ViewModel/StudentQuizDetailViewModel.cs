using static OnlineSinav.Controllers.QuizController;

namespace OnlineSinav.ViewModel
{
    public class StudentQuizDetailViewModel
    {
        public string QuizTitle { get; set; }
        public string StudentName { get; set; }
        public double Score { get; set; }
        public DateTime CompletedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public List<StudentAnswerDetailViewModel> Answers { get; set; }
    }
}
