using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneOtomasyon.API.Controllers;

public class DoktorlarController(
	IPersonelService personelService,
	IMusaitlikService musaitlikService,
	IRandevuService randevuService) : ApiControllerBase
{
	/// <summary>Doktor bilgisi, sunduğu hizmetler ve haftalık müsaitlikleri.</summary>
	[HttpGet("{id:int}")]
	[AllowAnonymous]
	public async Task<DoktorDetayDto> GetDetay(int id, CancellationToken ct) =>
		await personelService.GetDoktorDetayAsync(id, ct);

	/// <summary>Seçilen gün (yyyy-MM-dd) ve hizmet için boş randevu başlangıç saatleri; dilim süresi hizmetin süresidir.</summary>
	[HttpGet("{id:int}/uygun-saatler")]
	[AllowAnonymous]
	public async Task<IReadOnlyList<DateTime>> GetUygunSaatler(int id, [FromQuery] int hizmetId, [FromQuery] DateOnly tarih, CancellationToken ct) =>
		await randevuService.UygunSaatleriGetirAsync(id, hizmetId, tarih, ct);

	/// <summary>Hastaneye yeni doktor kaydeder.</summary>
	[HttpPost]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType<PersonelDto>(StatusCodes.Status201Created)]
	public async Task<IActionResult> Ekle(DoktorKayitDto dto, CancellationToken ct)
	{
		var doktor = await personelService.DoktorEkleAsync(dto, ct);
		return CreatedAtAction(nameof(GetDetay), new { id = doktor.Id }, doktor);
	}

	/// <summary>Doktorun poliklinik, uzmanlık ve unvan bilgisini günceller.</summary>
	[HttpPut("{id:int}")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Guncelle(int id, DoktorGuncelleDto dto, CancellationToken ct)
	{
		await personelService.DoktorGuncelleAsync(id, dto, ct);
		return NoContent();
	}

	/// <summary>Doktora sunacağı bir hizmeti atar (isteğe bağlı doktora özel ücretle).</summary>
	[HttpPost("{id:int}/hizmetler")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> HizmetAta(int id, DoktorHizmetAtaDto dto, CancellationToken ct)
	{
		await personelService.DoktorHizmetAtaAsync(id, dto, ct);
		return NoContent();
	}

	/// <summary>Doktordan bir hizmeti kaldırır.</summary>
	[HttpDelete("{id:int}/hizmetler/{hizmetId:int}")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> HizmetKaldir(int id, int hizmetId, CancellationToken ct)
	{
		await personelService.DoktorHizmetKaldirAsync(id, hizmetId, ct);
		return NoContent();
	}

	/// <summary>Doktorun haftalık müsaitlik dilimleri.</summary>
	[HttpGet("{id:int}/musaitlikler")]
	[AllowAnonymous]
	public async Task<IReadOnlyList<MusaitlikDto>> GetMusaitlikler(int id, CancellationToken ct) =>
		await musaitlikService.GetByDoktorAsync(id, ct);

	/// <summary>Doktora müsaitlik dilimi ekler (doktorun kendisi veya hastane yöneticisi).</summary>
	[HttpPost("{id:int}/musaitlikler")]
	[Authorize(Roles = RolGruplari.YonetimVeDoktor)]
	[ProducesResponseType<MusaitlikDto>(StatusCodes.Status201Created)]
	public async Task<IActionResult> MusaitlikEkle(int id, ZamanAraligiDto dto, CancellationToken ct) =>
		Olusturuldu(await musaitlikService.EkleAsync(id, dto, ct));

	/// <summary>Bir müsaitlik dilimini siler. Mevcut randevular etkilenmez.</summary>
	[HttpDelete("~/api/musaitlikler/{musaitlikId:int}")]
	[Authorize(Roles = RolGruplari.YonetimVeDoktor)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> MusaitlikSil(int musaitlikId, CancellationToken ct)
	{
		await musaitlikService.SilAsync(musaitlikId, ct);
		return NoContent();
	}

	/// <summary>Doktorun tarih aralığındaki randevuları (iki tarih dahil).</summary>
	[HttpGet("{id:int}/takvim")]
	[Authorize(Roles = RolGruplari.YonetimVeDoktor)]
	public async Task<IReadOnlyList<RandevuDto>> GetTakvim(int id, [FromQuery] DateOnly baslangic, [FromQuery] DateOnly bitis, CancellationToken ct) =>
		await randevuService.GetDoktorTakvimiAsync(id, baslangic, bitis, ct);
}
