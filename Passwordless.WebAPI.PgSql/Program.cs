using AzureDb.Passwordless.Postgresql;
using Microsoft.EntityFrameworkCore;
using Passwordless.WebAPI.PgSql.EF;
using Passwordless.WebAPI.PgSql.EF.Model;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ChecklistContext>(options =>
{
    AzureIdentityPostgresqlPasswordProvider passwordProvider = new AzureIdentityPostgresqlPasswordProvider();
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgopts =>
    {
        npgopts.ProvidePasswordCallback(passwordProvider.ProvidePasswordCallback);
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services, builder.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
