using Microsoft.EntityFrameworkCore;
using SPA_app.Hubs;
using SPA_app.Services.ElasticIndexIntialazer;
using SPA_приложение.Data;
using SPA_приложение.Extensions;
using SPA_приложение.Middleware;

namespace SPA_приложение
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCorsPolicies();

            builder.Services.AddApplicationServices();

            builder.Services.AddSignalR();

            builder.Services.AddControllers();

            builder.Services.AddApplicationValidators();

            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            var app = builder.Build();

			using (var scope = app.Services.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

				await db.Database.MigrateAsync();

                var initializer = scope.ServiceProvider.GetRequiredService<ElasticIndexInitializer>();

                await initializer.Initialize();

            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                //app.UseHttpsRedirection();
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("cors");

            app.UseAuthorization();
            app.MapControllers();

            app.MapGraphQL();
            app.MapHub<CommentsHub>("/commentsHub");

            app.MapFallbackToFile("index.html");

            await app.RunAsync();
        }
    }
}
