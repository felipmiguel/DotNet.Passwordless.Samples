namespace Passwordless.WebAPI.PgSql.Extensions
{
    public static class IConfigurationExtensions
    {
        public static string GetConnectionStringFallback(this IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection"); ;
            if (string.IsNullOrEmpty(connectionString))
            {
                System.Console.WriteLine("Connection string is empty");
                connectionString = configuration.GetSection("AZURE_POSTGRESQL_CONNECTIONSTRING").Value;
                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Console.WriteLine("Section AZURE_POSTGRESQL_CONNECTIONSTRING is empty");
                    connectionString = Environment.GetEnvironmentVariable("AZURE_POSTGRESQL_CONNECTIONSTRING");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        System.Console.WriteLine("Environment variable AZURE_POSTGRESQL_CONNECTIONSTRING is empty");
                        throw new Exception("Connection string is empty");
                    }
                }
            }

            return connectionString;
        }
    }
}
