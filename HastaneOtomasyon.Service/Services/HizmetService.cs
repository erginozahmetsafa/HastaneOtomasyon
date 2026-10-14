using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Core.UnitOfWorks;
using HastaneOtomasyon.Service.Extensions;
using HastaneOtomasyon.Service.Mapping;
using HastaneOtomasyon.Service.Security;

namespace HastaneOtomasyon.Service.Services;

internal sealed class HizmetService(
	IHizmetRepository hizmetRepository,
	IGenericRepository<Poliklinik> poliklinikRepository,
	IUnitOfWork unitOfWork,
	IAktifKullanici aktifKullanici) : IHizmetService
{
	public async Task<IReadOnlyList<HizmetDto>> GetByPoliklinikAsync(int poliklinikId, CancellationToken ct = default) =>
		(await hizmetRepository.GetByPoliklinikAsync(poliklinikId, ct: ct)).Select(h => h.ToDto()).ToList();

	public async Task<IReadOnlyList<HizmetDto>> GetByHastaneAsync(int hastaneId, CancellationToken ct = default) =>
		(await hizmetRepository.GetByHastaneAsync(hastaneId, ct: ct)).Select(h => h.ToDto()).ToList();

	public async Task<HizmetDto> OlusturAsync(HizmetOlusturDto dto, CancellationToken ct = default)
	{
		var poliklinik = await poliklinikRepository.GetByIdOrThrowAsync(dto.PoliklinikId, ct);
		aktifKullanici.HastaneYonetebilmeli(poliklinik.HastaneId);

		var hizmet = new Hizmet
		{
			PoliklinikId = poliklinik.Id,
			Ad = dto.Ad.Trim(),
			Aciklama = dto.Aciklama?.Trim(),
			SureDakika = dto.SureDakika,
			Ucret = dto.Ucret
		};

		await hizmetRepository.AddAsync(hizmet, ct);
		await unitOfWork.SaveChangesAsync(ct);

		return hizmet.ToDto();
	}

	// Süre/ücret değişikliği mevcut randevuları etkilemez; randevular kendi değerlerini saklar.
	public async Task GuncelleAsync(int id, HizmetGuncelleDto dto, CancellationToken ct = default)
	{
		var hizmet = await hizmetRepository.GetByIdOrThrowAsync(id, ct);
		var poliklinik = await poliklinikRepository.GetByIdOrThrowAsync(hizmet.PoliklinikId, ct);
		aktifKullanici.HastaneYonetebilmeli(poliklinik.HastaneId);

		hizmet.Ad = dto.Ad.Trim();
		hizmet.Aciklama = dto.Aciklama?.Trim();
		hizmet.SureDakika = dto.SureDakika;
		hizmet.Ucret = dto.Ucret;
		hizmet.AktifMi = dto.AktifMi;

		await unitOfWork.SaveChangesAsync(ct);
	}
}
