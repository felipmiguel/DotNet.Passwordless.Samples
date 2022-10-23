using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sample.Repository;
using System.Reflection;

namespace Passwordless.WebAPI.PgSql
{
    public class ChecklistContextFactory : IDesignTimeDbContextFactory<ChecklistContext>
    {
        public ChecklistContext CreateDbContext(string[] args)
        {
            Console.WriteLine("args {0}", string.Join(",", args));

            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Deployment.json");
            IConfigurationRoot config = configBuilder.Build();

            ServiceCollection services = new ServiceCollection();
            services.AddDbContext<ChecklistContext>(options =>
            {
                options.UseNpgsql(GetPGConnString(config), optionsBuilder =>
                optionsBuilder
                    .MigrationsAssembly(Assembly.GetExecutingAssembly().FullName)
                    .UseAadAuthentication());
            });

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<ChecklistContext>();
        }

        private string GetPGConnString(IConfiguration configuration)
        {
            return configuration.GetConnectionString("AZURE_POSTGRESQL_CONNECTIONSTRING");
        }
    }
}
