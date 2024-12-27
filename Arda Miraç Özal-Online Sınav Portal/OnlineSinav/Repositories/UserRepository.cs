using OnlineSinav.Models;
using Microsoft.EntityFrameworkCore;

namespace OnlineSinav.Repositories
{
    public class UserRepository : GenericRepository<AppUser>
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context, context.Users)
        {
            _context = context;
        }

        public async Task<List<AppUser>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Classroom)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AppUser> GetByIdAsync(string id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
