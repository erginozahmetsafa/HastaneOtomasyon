using HastaneOtomasyon.API.Models;
using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HastaneOtomasyon.API.Controllers;

public class HesapController(
	IHesapService hesapService,
	ITokenUretici tokenUretici,
	IAktifKullanici aktifKullanici) : ApiControllerBase
{
	/// <summary>Hasta olarak yeni hesap oluşturur ve oturum açar.</summary>
	[HttpPost("kayit")]
	[AllowAnonymous]
	[EnableRateLimiting(HizSinirlamaPolitikalari.HesapIslemleri)]
	[ProducesResponseType<GirisYanitiDto>(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
	public async Task<IActionResult> Kayit(HastaKayitDto dto, CancellationToken ct)
	{
		var oturum = await hesapService.HastaKayitAsync(dto, ct);
		return Olusturuldu(tokenUretici.Uret(oturum));
	}

	/// <summary>E-posta ve şifre ile giriş yapar; tüm roller için ortaktır. IP başına deneme sayısı sınırlıdır.</summary>
	[HttpPost("giris")]
	[AllowAnonymous]
	[EnableRateLimiting(HizSinirlamaPolitikalari.HesapIslemleri)]
	[ProducesResponseType<GirisYanitiDto>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
	public async Task<IActionResult> Giris(GirisDto dto, CancellationToken ct)
	{
		var oturum = await hesapService.GirisAsync(dto, ct);
		return oturum is null
			? Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Giriş başarısız", detail: "E-posta veya şifre hatalı.")
			: Ok(tokenUretici.Uret(oturum));
	}

	/// <summary>Şifreyi değiştirir. Diğer tüm oturumlar kapanır; yanıttaki yeni token kullanılmalıdır.</summary>
	[HttpPost("sifre-degistir")]
	[EnableRateLimiting(HizSinirlamaPolitikalari.HesapIslemleri)]
	[ProducesResponseType<GirisYanitiDto>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
	public async Task<IActionResult> SifreDegistir(SifreDegistirDto dto, CancellationToken ct) =>
		Ok(tokenUretici.Uret(await hesapService.SifreDegistirAsync(dto, ct)));

	/// <summary>Token sahibinin kimlik, rol ve hastane bilgisi.</summary>
	[HttpGet("ben")]
	public ActionResult<OturumDto> Ben() =>
		new OturumDto(aktifKullanici.Id!.Value, aktifKullanici.Rol!.Value, aktifKullanici.HastaneId);
}
