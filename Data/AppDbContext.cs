using DocumentManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Document> Documents => Set<Document>();
    }

}
