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
}
