using OnlineSinav.Models;

namespace OnlineSinav.ViewModel
{
    public class QuizDetailsViewModel
    {
        public Quiz Quiz { get; set; }
        public List<Question> Questions { get; set; }
    }
}
