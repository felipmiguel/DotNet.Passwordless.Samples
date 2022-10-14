using AzureDb.Passwordless.Postgresql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Passwordless.WebAPI.PgSql.EF.Model;
using Passwordless.WebAPI.PgSql.Extensions;

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
            //optionsBuilder.UseNpgsql("Server=postgres-passwordless.postgres.database.azure.com;Database=checklist;Port=5432;User Id=azureuser@postgres-passwordless;Ssl Mode=Require;Trust Server Certificate=true;Password=Corp123456789!");
            AzureIdentityPostgresqlPasswordProvider passwordProvider = new AzureIdentityPostgresqlPasswordProvider();
            optionsBuilder.UseNpgsql(configuration.GetConnectionStringFallback(), npgopts =>
            {
                npgopts.ProvidePasswordCallback(passwordProvider.ProvidePasswordCallback);
            });
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
