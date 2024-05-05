using Microsoft.EntityFrameworkCore;
using SharedProject.Models;

namespace SharedProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public virtual DbSet<Message> Messages { get; set; }
    }
}
