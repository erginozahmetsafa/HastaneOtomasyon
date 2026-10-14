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
}
