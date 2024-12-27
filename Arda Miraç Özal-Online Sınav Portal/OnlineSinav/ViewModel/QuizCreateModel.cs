namespace OnlineSinav.ViewModel
{
    public class QuizCreateModel
    {
        public string Title { get; set; }
        public int ClassroomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
