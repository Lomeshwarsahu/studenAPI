using Microsoft.EntityFrameworkCore;
using tsting_api.Models;

namespace tsting_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Student> Students { get; set; }
    }
}
