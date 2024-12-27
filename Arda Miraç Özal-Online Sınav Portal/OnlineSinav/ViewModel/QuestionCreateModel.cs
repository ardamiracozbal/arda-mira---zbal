using OnlineSinav.Controllers;

namespace OnlineSinav.ViewModel
{
    public class QuestionCreateModel
    {
        public int QuizId { get; set; }
        public string Text { get; set; }
        public List<OptionModel> Options { get; set; }
    }
}
