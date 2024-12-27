namespace OnlineSinav.ViewModel
{
    public class StudentResultViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public double Score { get; set; }
        public DateTime CompletedAt { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
    }
}
