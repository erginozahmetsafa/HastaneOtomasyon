using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneOtomasyon.API.Controllers;

public class PolikliniklerController(
	IPoliklinikService poliklinikService,
	IPersonelService personelService,
	IHizmetService hizmetService) : ApiControllerBase
{
	/// <summary>Poliklinikteki doktorlar.</summary>
	[HttpGet("{id:int}/doktorlar")]
	[AllowAnonymous]
	public async Task<IReadOnlyList<DoktorDto>> GetDoktorlar(int id, CancellationToken ct) =>
		await personelService.GetDoktorlarAsync(id, ct);

	/// <summary>Poliklinikte verilen aktif hizmetler.</summary>
	[HttpGet("{id:int}/hizmetler")]
	[AllowAnonymous]
	public async Task<IReadOnlyList<HizmetDto>> GetHizmetler(int id, CancellationToken ct) =>
		await hizmetService.GetByPoliklinikAsync(id, ct);

	/// <summary>Hastaneye yeni bölüm ekler.</summary>
	[HttpPost]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType<PoliklinikDto>(StatusCodes.Status201Created)]
	public async Task<IActionResult> Olustur(PoliklinikOlusturDto dto, CancellationToken ct) =>
		Olusturuldu(await poliklinikService.OlusturAsync(dto, ct));

	/// <summary>Poliklinik adını/açıklamasını günceller.</summary>
	[HttpPut("{id:int}")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Guncelle(int id, PoliklinikGuncelleDto dto, CancellationToken ct)
	{
		await poliklinikService.GuncelleAsync(id, dto, ct);
		return NoContent();
	}

	/// <summary>Personeli ve hizmeti olmayan polikliniği siler.</summary>
	[HttpDelete("{id:int}")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Sil(int id, CancellationToken ct)
	{
		await poliklinikService.SilAsync(id, ct);
		return NoContent();
	}
}
