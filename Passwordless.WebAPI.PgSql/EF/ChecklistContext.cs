using AzureDb.Passwordless.Postgresql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.WebAPI.PgSql.EF.Model;

namespace Passwordless.WebAPI.PgSql.EF
{
    public class ChecklistContext : DbContext
    {
        private readonly IConfiguration configuration;

        public DbSet<Checklist> Checklists { get; set; }
        public DbSet<CheckItem> CheckItems { get; set; }
        public ChecklistContext(DbContextOptions<ChecklistContext> options, IConfiguration configuration) : base(options)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            AzureIdentityPostgresqlPasswordProvider passwordProvider = new AzureIdentityPostgresqlPasswordProvider();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgopts =>
            {
                npgopts.ProvidePasswordCallback(passwordProvider.ProvidePasswordCallback);
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Checklist>()
                .HasKey(c => c.ID);
            modelBuilder.Entity<CheckItem>()
                .HasKey(ci => ci.ID);
            modelBuilder.Entity<Checklist>()
                .HasMany(c => c.CheckItems)
                .WithOne(ci => ci.Checklist)
                .HasForeignKey(ci => ci.ChecklistID);
        }
    }
}
