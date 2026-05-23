using Microsoft.EntityFrameworkCore;
using SPA_app.Extensions.Init;
using SPA_app.Services.ElasticIndexIntialazer;
using SPA_приложение.Data;

namespace SPA_app.Extensions
{
	public static class InitializeServices
	{
		public static async Task InitService(WebApplication app)
		{
			using (var scope = app.Services.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
				await db.Database.MigrateAsync();

				var initializer = scope.ServiceProvider.GetRequiredService<ElasticSearchInitializer>();
				await initializer.Initialize();

				var init = scope.ServiceProvider.GetRequiredService<RabbitMQInit>();
				init.Init();
			}
		}
	}
}
