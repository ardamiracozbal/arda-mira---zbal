using OnlineSinav.Models;
using Microsoft.EntityFrameworkCore;

namespace OnlineSinav.Repositories
{
    public class QuizRepository : GenericRepository<Quiz>
    {
        private readonly AppDbContext _context;

        public QuizRepository(AppDbContext context) : base(context, context.Quizzes)
        {
            _context = context;
        }

        public async Task<List<Quiz>> GetAllWithDetailsAsync()
        {
            return await _context.Quizzes
                .Include(q => q.Teacher)
                .Include(q => q.Classroom)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .ToListAsync();
        }

        public async Task<Quiz> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Teacher)
                .Include(q => q.Classroom)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<Quiz>> GetQuizzesByClassroomId(int classroomId)
        {
            return await _context.Quizzes
                .Include(q => q.Classroom)
                .Include(q => q.Questions)
                .Where(q => q.ClassroomId == classroomId && q.IsActive)
                .OrderByDescending(q => q.StartDate)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetQuizzesByTeacherIdAsync(string teacherId)
        {
            return await _context.Quizzes
                .Include(q => q.Classroom)
                .Include(q => q.Questions)
                .Include(q => q.Teacher)
                .Where(q => q.TeacherId == teacherId)
                .OrderByDescending(q => q.StartDate)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetActiveQuizzesByClassroomIdAsync(int? classroomId)
        {
            if (!classroomId.HasValue)
                return new List<Quiz>();

            return await _context.Quizzes
                .Include(q => q.Classroom)
                .Include(q => q.Questions)
                .Where(q => q.ClassroomId == classroomId.Value && 
                            q.IsActive && 
                            q.StartDate <= DateTime.Now && 
                            q.EndDate >= DateTime.Now)
                .OrderBy(q => q.EndDate)
                .ToListAsync();
        }
    }
} 