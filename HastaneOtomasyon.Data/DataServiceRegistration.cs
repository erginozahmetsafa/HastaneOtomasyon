using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.UnitOfWorks;
using HastaneOtomasyon.Data.Repositories;
using HastaneOtomasyon.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HastaneOtomasyon.Data;

public static class DataServiceRegistration
{
	public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
	{
		services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

		services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
		services.AddScoped<IKisiRepository, KisiRepository>();
		services.AddScoped<IHastaneRepository, HastaneRepository>();
		services.AddScoped<IHizmetRepository, HizmetRepository>();
		services.AddScoped<IDoktorRepository, DoktorRepository>();
		services.AddScoped<IMusaitlikRepository, MusaitlikRepository>();
		services.AddScoped<IRandevuRepository, RandevuRepository>();
		services.AddScoped<IUnitOfWork, UnitOfWork>();

		return services;
	}

	// Bekleyen migration'ları uygular. Geliştirme ortamında uygulama açılışında çağrılır.
	public static async Task VeritabaniniGuncelleAsync(this IServiceProvider services, CancellationToken ct = default)
	{
		await using var scope = services.CreateAsyncScope();
		await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync(ct);
	}
}
