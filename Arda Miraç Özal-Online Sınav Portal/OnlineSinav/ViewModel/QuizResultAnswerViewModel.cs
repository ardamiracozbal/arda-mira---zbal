namespace OnlineSinav.ViewModel
{
    public class QuizResultAnswerViewModel
    {
        public string QuestionText { get; set; }
        public string SelectedOptionText { get; set; }
        public bool IsCorrect { get; set; }
        public string CorrectOptionText { get; set; }
    }
}
