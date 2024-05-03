using Microsoft.EntityFrameworkCore;
using SharedProject.Models;

namespace SharedProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<Message> Messages { get; set; }
    }
}
