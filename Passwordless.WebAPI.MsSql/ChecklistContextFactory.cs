using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sample.Repository;
using System.Reflection;

namespace Passwordless.WebAPI.MsSql
{
    public class ChecklistContextFactory : IDesignTimeDbContextFactory<ChecklistContext>
    {
        public ChecklistContext CreateDbContext(string[] args)
        {
            Console.WriteLine("args {0}", string.Join(",", args));

            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder
                .AddJsonFile("appsettings.json");
            IConfigurationRoot config = configBuilder.Build();

            ServiceCollection services = new ServiceCollection();
            
            services.AddDbContext<ChecklistContext>(options =>
            {
                string connString = GetConnectionString(config);
                Console.WriteLine("connString {0}", connString);
                options.UseSqlServer(connString,
                    optionsBuilder => optionsBuilder.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName));
            });

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<ChecklistContext>();
        }

        private static string? GetConnectionString(IConfiguration configuration)
        {
            Console.WriteLine("looking for conn string...");
            var connString = configuration.GetSection("AZURE_MSSQL_CONNECTIONSTRING").Value;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connString)
            {
                Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated,
                TrustServerCertificate=true,
                ConnectTimeout=30
            };
            return builder.ConnectionString;
            
        }
    }
}
