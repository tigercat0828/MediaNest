using MediaNest.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaNest.Web.Database; 
public class AppDbContext : DbContext {
    // XxxDbContext
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        Database.EnsureCreated();
    }
    public DbSet<Game> Games { get; set; }
}
