using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sample.Repository;
using System.Reflection;

namespace Passwordless.WebAPI.MySql
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
                string connectionString = GetConnectionString(config);
                Console.WriteLine("connectionString {0}", connectionString);
                var serverVersion = ServerVersion.Parse("5.7", Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql);
                options.UseMySql(connectionString, serverVersion,
                    optionsBuilder => optionsBuilder.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName))
                .UseAadAuthentication();
            });

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<ChecklistContext>();
        }

        private static string? GetConnectionString(IConfiguration configuration)
        {
            Console.WriteLine("looking for conn string...");
            //return configuration.GetConnectionString("AZURE_MYSQL_CONNECTIONSTRING");
            return configuration.GetSection("mysqlconnstring").Value;
        }
    }
}
