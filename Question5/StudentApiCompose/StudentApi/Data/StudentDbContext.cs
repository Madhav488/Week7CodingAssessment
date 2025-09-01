
using Microsoft.EntityFrameworkCore;
using StudentApi.Models;

namespace StudentApi.Data
{
    public class StudentDbContext : DbContext
    {
        public StudentDbContext(DbContextOptions<StudentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(b =>
            {
                b.HasKey(s => s.Rn);
                b.Property(s => s.Name).HasMaxLength(100).IsRequired();
                b.Property(s => s.Batch).HasMaxLength(50).IsRequired();
                b.Property(s => s.Marks).IsRequired();
            });
        }
    }
}
