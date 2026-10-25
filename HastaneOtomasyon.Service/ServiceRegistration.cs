using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Service.Security;
using HastaneOtomasyon.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HastaneOtomasyon.Service;

public static class ServiceRegistration
{
	// IAktifKullanici gerçekleştirimi sunum katmanı (API) tarafından ayrıca kaydedilmelidir.
	public static IServiceCollection AddServiceLayer(this IServiceCollection services)
	{
		services.TryAddSingleton(TimeProvider.System);
		services.AddSingleton<ISifreHasher, SifreHasher>();
		services.AddScoped<KisiFabrikasi>();

		services.AddScoped<IHesapService, HesapService>();
		services.AddScoped<IHastaneService, HastaneService>();
		services.AddScoped<IPoliklinikService, PoliklinikService>();
		services.AddScoped<IHizmetService, HizmetService>();
		services.AddScoped<IPersonelService, PersonelService>();
		services.AddScoped<IMusaitlikService, MusaitlikService>();
		services.AddScoped<IRandevuService, RandevuService>();

		services.AddScoped<DemoVeriTohumlayici>();

		return services;
	}

	// Geliştirme ortamında örnek kurum ve hesapları ekler (bkz. DemoVeriTohumlayici). Tekrar çağrılması güvenlidir.
	public static async Task DemoVerileriniEkleAsync(this IServiceProvider services, CancellationToken ct = default)
	{
		await using var scope = services.CreateAsyncScope();
		await scope.ServiceProvider.GetRequiredService<DemoVeriTohumlayici>().TohumlaAsync(ct);
	}
}
