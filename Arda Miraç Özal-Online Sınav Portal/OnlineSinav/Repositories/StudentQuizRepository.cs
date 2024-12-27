using Microsoft.EntityFrameworkCore;
using OnlineSinav.Models;

namespace OnlineSinav.Repositories
{
    public class StudentQuizRepository : GenericRepository<StudentQuiz>
    {
        private readonly AppDbContext _context;

        public StudentQuizRepository(AppDbContext context) : base(context, context.StudentQuizzes)
        {
            _context = context;
        }

        public async Task<StudentQuiz> GetByStudentAndQuizId(string studentId, int quizId)
        {
            return await _context.StudentQuizzes
                .Include(sq => sq.Answers)
                .FirstOrDefaultAsync(sq => sq.StudentId == studentId && sq.QuizId == quizId);
        }

        public async Task<bool> HasStudentCompletedQuiz(string studentId, int quizId)
        {
            var studentQuiz = await GetByStudentAndQuizId(studentId, quizId);
            return studentQuiz?.IsCompleted ?? false;
        }

        public async Task<StudentQuiz> StartQuiz(string studentId, int quizId)
        {
            var studentQuiz = new StudentQuiz
            {
                StudentId = studentId,
                QuizId = quizId,
                StartedAt = DateTime.UtcNow,
                IsCompleted = false
            };

            await AddAsync(studentQuiz);
            return studentQuiz;
        }

        public async Task SaveAnswer(int studentQuizId, int questionId, int selectedOptionId)
        {
            var answer = new StudentAnswer
            {
                StudentQuizId = studentQuizId,
                QuestionId = questionId,
                SelectedOptionId = selectedOptionId
            };

            _context.StudentAnswers.Add(answer);
            await _context.SaveChangesAsync();
        }

        public async Task CompleteQuiz(int studentQuizId)
        {
            var studentQuiz = await _context.StudentQuizzes
      .Include(sq => sq.Answers) // Answers'ý dahil et
          .ThenInclude(a => a.Question) // Sorularý dahil et
              .ThenInclude(q => q.Options) // Seçenekleri dahil et
      .FirstOrDefaultAsync(sq => sq.Id == studentQuizId);

            if (studentQuiz != null)
            {
                var totalQuestions = studentQuiz.Answers.Count;

                // Toplam soru sayýsý sýfýrsa, skoru hesaplamadan çýk
                if (totalQuestions == 0)
                {
                    studentQuiz.Score = 0; // Veya uygun bir deðer atayýn
                    studentQuiz.CompletedAt = DateTime.UtcNow;
                    studentQuiz.IsCompleted = true;

                    await UpdateAsync(studentQuiz);
                    return;
                }

                var correctAnswers = studentQuiz.Answers.Count(a =>
                    a.Question.Options.FirstOrDefault(o => o.Id == a.SelectedOptionId)?.IsCorrect == true);

                studentQuiz.Score = (double)correctAnswers / totalQuestions * 100;
                studentQuiz.CompletedAt = DateTime.UtcNow;
                studentQuiz.IsCompleted = true;

                await UpdateAsync(studentQuiz);
            }
        }

        public async Task<StudentQuiz> GetQuizResultAsync(string userId, int quizId)
        {
            return await _context.StudentQuizzes
                .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.Questions)
                .Include(sq => sq.Answers)
                    .ThenInclude(a => a.Question)
                        .ThenInclude(q => q.Options)
                .Include(sq => sq.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(sq => 
                    sq.StudentId == userId && 
                    sq.QuizId == quizId && 
                    sq.IsCompleted);
        }

        public async Task<List<StudentQuiz>> GetQuizResultsAsync(int quizId)
        {
            return await _context.StudentQuizzes
                .Include(sq => sq.Student)
                .Include(sq => sq.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .Where(sq => sq.QuizId == quizId && sq.IsCompleted)
                .ToListAsync();
        }

        public async Task<StudentQuiz> GetStudentQuizDetailAsync(int quizId, string studentId)
        {
            return await _context.StudentQuizzes
                .Include(sq => sq.Student)
                .Include(sq => sq.Answers)
                    .ThenInclude(a => a.Question)
                        .ThenInclude(q => q.Options)
                .Include(sq => sq.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(sq => 
                    sq.QuizId == quizId && 
                    sq.StudentId == studentId && 
                    sq.IsCompleted);
        }

        public async Task<double> CalculateScore(int quizId, string studentId)
        {
            var studentQuiz = await _context.StudentQuizzes
                .Include(sq => sq.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(sq => 
                    sq.QuizId == quizId && 
                    sq.StudentId == studentId && 
                    sq.IsCompleted);

            if (studentQuiz == null)
                return 0;

            var totalQuestions = studentQuiz.Quiz.Questions.Count;
            if (totalQuestions == 0)
                return 0;

            var correctAnswers = studentQuiz.Answers
                .Count(a => a.SelectedOption != null && a.SelectedOption.IsCorrect);

            return Math.Round((double)correctAnswers / totalQuestions * 100, 2);
        }
    }
} 