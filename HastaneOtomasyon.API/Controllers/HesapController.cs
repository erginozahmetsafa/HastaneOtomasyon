using HastaneOtomasyon.API.Models;
using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneOtomasyon.API.Controllers;

public class HesapController(
	IHesapService hesapService,
	ITokenUretici tokenUretici,
	IAktifKullanici aktifKullanici) : ApiControllerBase
{
	/// <summary>Hasta olarak yeni hesap oluşturur ve oturum açar.</summary>
	[HttpPost("kayit")]
	[AllowAnonymous]
	[ProducesResponseType<GirisYanitiDto>(StatusCodes.Status201Created)]
	public async Task<IActionResult> Kayit(HastaKayitDto dto, CancellationToken ct)
	{
		var kullanici = await hesapService.HastaKayitAsync(dto, ct);
		return Olusturuldu(tokenUretici.Uret(kullanici));
	}

	/// <summary>E-posta ve şifre ile giriş yapar; tüm roller için ortaktır.</summary>
	[HttpPost("giris")]
	[AllowAnonymous]
	[ProducesResponseType<GirisYanitiDto>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Giris(GirisDto dto, CancellationToken ct)
	{
		var kullanici = await hesapService.GirisAsync(dto, ct);
		return kullanici is null
			? Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Giriş başarısız", detail: "E-posta veya şifre hatalı.")
			: Ok(tokenUretici.Uret(kullanici));
	}

	/// <summary>Token sahibinin kimlik, rol ve hastane bilgisi.</summary>
	[HttpGet("ben")]
	public ActionResult<OturumDto> Ben() =>
		new OturumDto(aktifKullanici.Id!.Value, aktifKullanici.Rol!.Value, aktifKullanici.HastaneId);
}
