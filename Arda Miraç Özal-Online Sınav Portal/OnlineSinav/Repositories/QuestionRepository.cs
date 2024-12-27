using Microsoft.EntityFrameworkCore;
using OnlineSinav.Models;

namespace OnlineSinav.Repositories
{
    public class QuestionRepository : GenericRepository<Question>
    {
        private readonly AppDbContext _context;

        public QuestionRepository(AppDbContext context) : base(context, context.Questions)
        {
            _context = context;
        }

        public async Task<Question> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<Question>> GetQuestionsByQuizIdAsync(int quizId)
        {
            return await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.QuizId == quizId)
                .ToListAsync();
        }

        public async Task<bool> DeleteQuestionWithOptionsAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Soruyu ve ilişkili şıklarını getir
                var question = await _context.Questions
                    .Include(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (question == null)
                    return false;

                // Önce şıkları sil
                if (question.Options != null && question.Options.Any())
                {
                    _context.Options.RemoveRange(question.Options);
                }

                // Sonra soruyu sil
                _context.Questions.Remove(question);
                
                // Değişiklikleri kaydet
                await _context.SaveChangesAsync();
                
                // Transaction'ı commit et
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                // Hata durumunda transaction'ı geri al
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
} 