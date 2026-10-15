using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HastaneOtomasyon.API.Controllers;

public class RandevularController(IRandevuService randevuService) : ApiControllerBase
{
	/// <summary>Giriş yapan hasta adına randevu oluşturur. Randevu "Bekliyor" durumunda başlar.</summary>
	[HttpPost]
	[Authorize(Roles = RolGruplari.Hasta)]
	[ProducesResponseType<RandevuDto>(StatusCodes.Status201Created)]
	public async Task<IActionResult> Olustur(RandevuOlusturDto dto, CancellationToken ct)
	{
		var randevu = await randevuService.OlusturAsync(dto, ct);
		return CreatedAtAction(nameof(GetDetay), new { id = randevu.Id }, randevu);
	}

	/// <summary>Giriş yapan hastanın randevuları (yeniden eskiye).</summary>
	[HttpGet("benim")]
	[Authorize(Roles = RolGruplari.Hasta)]
	public async Task<IReadOnlyList<RandevuDto>> GetBenim(CancellationToken ct) =>
		await randevuService.GetBenimRandevularimAsync(ct);

	/// <summary>Randevu detayı (randevunun hastası, doktoru veya hastane yöneticisi).</summary>
	[HttpGet("{id:int}")]
	public async Task<RandevuDto> GetDetay(int id, CancellationToken ct) =>
		await randevuService.GetDetayAsync(id, ct);

	/// <summary>Bekleyen randevuyu onaylar.</summary>
	[HttpPost("{id:int}/onayla")]
	[Authorize(Roles = RolGruplari.YonetimVeDoktor)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Onayla(int id, CancellationToken ct)
	{
		await randevuService.OnaylaAsync(id, ct);
		return NoContent();
	}

	/// <summary>Bekleyen randevuyu gerekçesiyle reddeder.</summary>
	[HttpPost("{id:int}/reddet")]
	[Authorize(Roles = RolGruplari.YonetimVeDoktor)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Reddet(int id, RedDto dto, CancellationToken ct)
	{
		await randevuService.ReddetAsync(id, dto.Neden, ct);
		return NoContent();
	}

	/// <summary>Başlamamış randevuyu iptal eder (hasta, doktor veya yönetici). Gövde isteğe bağlıdır.</summary>
	[HttpPost("{id:int}/iptal")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> IptalEt(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] IptalDto? dto, CancellationToken ct)
	{
		await randevuService.IptalEtAsync(id, dto?.Neden, ct);
		return NoContent();
	}

	/// <summary>Onaylı ve saati gelmiş randevuyu tamamlandı olarak işaretler.</summary>
	[HttpPost("{id:int}/tamamla")]
	[Authorize(Roles = RolGruplari.YonetimVeDoktor)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Tamamla(int id, CancellationToken ct)
	{
		await randevuService.TamamlaAsync(id, ct);
		return NoContent();
	}
}
