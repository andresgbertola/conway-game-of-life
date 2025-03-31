using GOL.Domain.Entities;
using GOL.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace GOL.Infrastructure.Data
{
    public class GOLDbContext : DbContext
    {
        public DbSet<BoardState> BoardStates { get; set; }


        public GOLDbContext(DbContextOptions<GOLDbContext> options) : base(options)
        {   
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new BoardStateMap(modelBuilder.Entity<BoardState>().ToTable("BoardStates"));
            base.OnModelCreating(modelBuilder);
        }
    }
}
