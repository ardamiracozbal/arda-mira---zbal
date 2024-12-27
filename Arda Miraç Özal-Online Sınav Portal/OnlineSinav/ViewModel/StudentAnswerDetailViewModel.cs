using static OnlineSinav.Controllers.QuizController;

namespace OnlineSinav.ViewModel
{
    public class StudentAnswerDetailViewModel
    {
        public string QuestionText { get; set; }
        public string SelectedOptionText { get; set; }
        public bool IsCorrect { get; set; }
        public string CorrectOptionText { get; set; }
        public List<OptionDetailViewModel> AllOptions { get; set; }
    }
}
