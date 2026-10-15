using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneOtomasyon.API.Controllers;

public class HizmetlerController(
	IHizmetService hizmetService,
	IPersonelService personelService) : ApiControllerBase
{
	/// <summary>Bu hizmeti sunan doktorlar (randevu akışında "işlem seç → doktor seç").</summary>
	[HttpGet("{id:int}/doktorlar")]
	[AllowAnonymous]
	public async Task<IReadOnlyList<DoktorDto>> GetDoktorlar(int id, CancellationToken ct) =>
		await personelService.GetDoktorlarByHizmetAsync(id, ct);

	/// <summary>Polikliniğe süre ve ücretiyle yeni hizmet tanımlar.</summary>
	[HttpPost]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType<HizmetDto>(StatusCodes.Status201Created)]
	public async Task<IActionResult> Olustur(HizmetOlusturDto dto, CancellationToken ct) =>
		Olusturuldu(await hizmetService.OlusturAsync(dto, ct));

	/// <summary>Hizmeti günceller; AktifMi = false ile kullanımdan kaldırır. Mevcut randevular etkilenmez.</summary>
	[HttpPut("{id:int}")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Guncelle(int id, HizmetGuncelleDto dto, CancellationToken ct)
	{
		await hizmetService.GuncelleAsync(id, dto, ct);
		return NoContent();
	}
}
