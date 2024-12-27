using OnlineSinav.Models;

namespace OnlineSinav.Repositories
{
    public class ClassroomRepository : GenericRepository<Classroom>
    {
        public ClassroomRepository(AppDbContext context) : base(context, context.Set<Classroom>())
        {
        }

        // Ekstra metodlar eklemek isterseniz buraya ekleyebilirsiniz.
    }
} 