using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneOtomasyon.API.Controllers;

public class HastanelerController(
	IHastaneService hastaneService,
	IPoliklinikService poliklinikService,
	IHizmetService hizmetService,
	IPersonelService personelService,
	IRandevuService randevuService) : ApiControllerBase
{
	/// <summary>Tüm hastaneleri/şubeleri listeler.</summary>
	[HttpGet]
	[AllowAnonymous]
	public async Task<IReadOnlyList<HastaneDto>> GetAll(CancellationToken ct) =>
		await hastaneService.GetAllAsync(ct);

	/// <summary>Hastane bilgileri, bölümleri ve çalışma saatleri.</summary>
	[HttpGet("{id:int}")]
	[AllowAnonymous]
	public async Task<HastaneDetayDto> GetDetay(int id, CancellationToken ct) =>
		await hastaneService.GetDetayAsync(id, ct);

	/// <summary>Hastanenin poliklinikleri (bölümleri).</summary>
	[HttpGet("{id:int}/poliklinikler")]
	[AllowAnonymous]
	public async Task<IReadOnlyList<PoliklinikDto>> GetPoliklinikler(int id, CancellationToken ct) =>
		await poliklinikService.GetByHastaneAsync(id, ct);

	/// <summary>
	/// Hastanenin sunduğu aktif hizmetler (süre ve ücretleriyle).
	/// pasiflerDahil=true ile kullanımdan kaldırılanlar da döner; bu durumda hastane yöneticisi yetkisi gerekir.
	/// </summary>
	[HttpGet("{id:int}/hizmetler")]
	[AllowAnonymous]
	public async Task<IReadOnlyList<HizmetDto>> GetHizmetler(int id, [FromQuery] bool pasiflerDahil, CancellationToken ct) =>
		await hizmetService.GetByHastaneAsync(id, pasiflerDahil, ct);

	/// <summary>Yeni hastane/şube tanımlar.</summary>
	[HttpPost]
	[Authorize(Roles = RolGruplari.SistemYoneticisi)]
	[ProducesResponseType<HastaneDto>(StatusCodes.Status201Created)]
	public async Task<IActionResult> Olustur(HastaneKaydetDto dto, CancellationToken ct)
	{
		var hastane = await hastaneService.OlusturAsync(dto, ct);
		return CreatedAtAction(nameof(GetDetay), new { id = hastane.Id }, hastane);
	}

	/// <summary>Hastane bilgilerini günceller.</summary>
	[HttpPut("{id:int}")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Guncelle(int id, HastaneKaydetDto dto, CancellationToken ct)
	{
		await hastaneService.GuncelleAsync(id, dto, ct);
		return NoContent();
	}

	/// <summary>Boş bir hastaneyi siler (polikliniği veya çalışanı varsa silinemez).</summary>
	[HttpDelete("{id:int}")]
	[Authorize(Roles = RolGruplari.SistemYoneticisi)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Sil(int id, CancellationToken ct)
	{
		await hastaneService.SilAsync(id, ct);
		return NoContent();
	}

	/// <summary>Hastaneye bir günün çalışma saatini ekler.</summary>
	[HttpPost("{id:int}/calisma-saatleri")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType<CalismaSaatiDto>(StatusCodes.Status201Created)]
	public async Task<IActionResult> CalismaSaatiEkle(int id, ZamanAraligiDto dto, CancellationToken ct) =>
		Olusturuldu(await hastaneService.CalismaSaatiEkleAsync(id, dto, ct));

	/// <summary>Bir çalışma saati kaydını siler.</summary>
	[HttpDelete("~/api/calisma-saatleri/{calismaSaatiId:int}")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> CalismaSaatiSil(int calismaSaatiId, CancellationToken ct)
	{
		await hastaneService.CalismaSaatiSilAsync(calismaSaatiId, ct);
		return NoContent();
	}

	/// <summary>Hastanenin tüm çalışanları (yönetici, doktor, sağlık personeli).</summary>
	[HttpGet("{id:int}/personeller")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	public async Task<IReadOnlyList<PersonelDto>> GetPersoneller(int id, CancellationToken ct) =>
		await personelService.GetPersonellerAsync(id, ct);

	/// <summary>Hastanedeki onay bekleyen randevular.</summary>
	[HttpGet("{id:int}/onay-bekleyen-randevular")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	public async Task<IReadOnlyList<RandevuDto>> GetOnayBekleyenler(int id, CancellationToken ct) =>
		await randevuService.GetOnayBekleyenlerAsync(id, ct);
}
