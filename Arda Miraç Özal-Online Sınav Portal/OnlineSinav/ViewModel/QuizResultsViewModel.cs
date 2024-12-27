using static OnlineSinav.Controllers.QuizController;

namespace OnlineSinav.ViewModel
{
    public class QuizResultsViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public string ClassroomName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalQuestions { get; set; }
        public List<StudentResultViewModel> Results { get; set; }

        public double AverageScore => Results.Any() ? Results.Average(r => r.Score) : 0;
        public int TotalStudents => Results.Count;
        public int PassingStudents => Results.Count(r => r.Score >= 50);
    }
}
