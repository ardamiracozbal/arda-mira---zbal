using static OnlineSinav.Controllers.QuizController;

namespace OnlineSinav.ViewModel
{
    public class QuizSubmissionModel
    {
        public int StudentQuizId { get; set; }
        public List<AnswerModel> Answers { get; set; }
    }
}
