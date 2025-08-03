using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure CORS ώστε να επιτρέπεις αιτήματα από το BlazorClient
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                      .WithOrigins("https://localhost:7108")   // το origin του BlazorClient
                      .AllowAnyHeader()
                      .AllowAnyMethod();
                });
            });

            // Ανάγνωση connection string
            var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

            // Καταχώρηση DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connStr).ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning)));

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 3) Use CORS **πριν** τα endpoints
            app.UseCors();

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
        }
    }
}
