using Microsoft.EntityFrameworkCore;
using TaskCoreApi.Model;

namespace TaskCoreApi.EFDbContext
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options):base(options)
        {

        }
        public DbSet<User> Users { get; set; }
    }

   
}
