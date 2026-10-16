using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneOtomasyon.API.Controllers;

// Doktor kaydı DoktorlarController'dadır; burada diğer çalışan türleri bulunur.
public class PersonelController(IPersonelService personelService) : ApiControllerBase
{
	/// <summary>Hastaneye sağlık personeli (hemşire, laborant vb.) kaydeder.</summary>
	[HttpPost("saglik-personeli")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType<PersonelDto>(StatusCodes.Status201Created)]
	public async Task<IActionResult> SaglikPersoneliEkle(SaglikPersoneliKayitDto dto, CancellationToken ct) =>
		Olusturuldu(await personelService.SaglikPersoneliEkleAsync(dto, ct));

	/// <summary>Bir hastaneye yönetici atar.</summary>
	[HttpPost("yoneticiler")]
	[Authorize(Roles = RolGruplari.SistemYoneticisi)]
	[ProducesResponseType<PersonelDto>(StatusCodes.Status201Created)]
	public async Task<IActionResult> YoneticiEkle(YoneticiKayitDto dto, CancellationToken ct) =>
		Olusturuldu(await personelService.YoneticiEkleAsync(dto, ct));

	/// <summary>
	/// Çalışanı pasifleştirir: giriş yapamaz, açık oturumları kapanır, doktorsa listelerde görünmez
	/// ve ileri tarihli randevuları iptal edilir. Kayıt ve randevu geçmişi korunur.
	/// </summary>
	[HttpPost("{id:int}/pasiflestir")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	public async Task<PasiflestirmeSonucuDto> Pasiflestir(int id, CancellationToken ct) =>
		await personelService.PasiflestirAsync(id, ct);

	/// <summary>Pasif çalışanı yeniden aktifleştirir.</summary>
	[HttpPost("{id:int}/aktiflestir")]
	[Authorize(Roles = RolGruplari.Yonetim)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Aktiflestir(int id, CancellationToken ct)
	{
		await personelService.AktiflestirAsync(id, ct);
		return NoContent();
	}
}
