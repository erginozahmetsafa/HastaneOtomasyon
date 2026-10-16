using System.Security.Claims;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace HastaneOtomasyon.API.Security;

// İmzası ve süresi geçerli bir token'ın hâlâ kullanılabilir olup olmadığını kontrol eder.
// Hesap pasifleştirildiğinde veya şifre değiştirildiğinde eski token'lar süresi dolmadan reddedilir.
internal static class OturumDogrulama
{
	public static async Task TokenDogrulandiAsync(TokenValidatedContext context)
	{
		var kullanici = context.Principal;

		var gecerli = kullanici is not null
			&& int.TryParse(kullanici.FindFirstValue(ClaimTipleri.Id), out var kisiId)
			&& Guid.TryParse(kullanici.FindFirstValue(ClaimTipleri.GuvenlikDamgasi), out var damga)
			&& await context.HttpContext.RequestServices.GetRequiredService<IHesapService>()
				.OturumGecerliMiAsync(kisiId, damga, context.HttpContext.RequestAborted);

		if (!gecerli)
			context.Fail("Oturum artık geçerli değil (hesap pasif veya şifre değiştirilmiş).");
	}
}
