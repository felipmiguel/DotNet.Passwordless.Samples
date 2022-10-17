using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Passwordless.WebAPI.PgSql.EF.Model;

namespace Passwordless.WebAPI.PgSql.EF
{
    public class ChecklistContext : DbContext
    {

        public DbSet<Checklist>? Checklists { get; set; }
        public DbSet<CheckItem>? CheckItems { get; set; }
        
        public ChecklistContext(DbContextOptions<ChecklistContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Checklist>()
                .HasKey(c => c.ID)
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            modelBuilder.Entity<CheckItem>()
                .HasKey(ci => ci.ID)
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            modelBuilder.Entity<Checklist>()
                .HasMany(c => c.CheckItems)
                .WithOne(ci => ci.Checklist)
                .HasForeignKey(ci => ci.ChecklistID);
        }
    }
}
