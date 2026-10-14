using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Core.UnitOfWorks;
using HastaneOtomasyon.Service.Extensions;
using HastaneOtomasyon.Service.Mapping;
using HastaneOtomasyon.Service.Security;

namespace HastaneOtomasyon.Service.Services;

internal sealed class PoliklinikService(
	IGenericRepository<Poliklinik> poliklinikRepository,
	IHastaneRepository hastaneRepository,
	IDoktorRepository doktorRepository,
	IGenericRepository<SaglikPersoneli> saglikPersoneliRepository,
	IHizmetRepository hizmetRepository,
	IUnitOfWork unitOfWork,
	IAktifKullanici aktifKullanici) : IPoliklinikService
{
	public async Task<IReadOnlyList<PoliklinikDto>> GetByHastaneAsync(int hastaneId, CancellationToken ct = default)
	{
		var poliklinikler = await poliklinikRepository.GetAllAsync(p => p.HastaneId == hastaneId, ct);
		return poliklinikler.OrderBy(p => p.Ad).Select(p => p.ToDto()).ToList();
	}

	public async Task<PoliklinikDto> OlusturAsync(PoliklinikOlusturDto dto, CancellationToken ct = default)
	{
		aktifKullanici.HastaneYonetebilmeli(dto.HastaneId);
		await hastaneRepository.VarOlmaliAsync(dto.HastaneId, ct);

		var ad = dto.Ad.Trim();
		await AdBenzersizOlmaliAsync(dto.HastaneId, ad, haricId: null, ct);

		var poliklinik = new Poliklinik { HastaneId = dto.HastaneId, Ad = ad, Aciklama = dto.Aciklama?.Trim() };
		await poliklinikRepository.AddAsync(poliklinik, ct);
		await unitOfWork.SaveChangesAsync(ct);

		return poliklinik.ToDto();
	}

	public async Task GuncelleAsync(int id, PoliklinikGuncelleDto dto, CancellationToken ct = default)
	{
		var poliklinik = await poliklinikRepository.GetByIdOrThrowAsync(id, ct);
		aktifKullanici.HastaneYonetebilmeli(poliklinik.HastaneId);

		var ad = dto.Ad.Trim();
		await AdBenzersizOlmaliAsync(poliklinik.HastaneId, ad, haricId: id, ct);

		poliklinik.Ad = ad;
		poliklinik.Aciklama = dto.Aciklama?.Trim();
		await unitOfWork.SaveChangesAsync(ct);
	}

	public async Task SilAsync(int id, CancellationToken ct = default)
	{
		var poliklinik = await poliklinikRepository.GetByIdOrThrowAsync(id, ct);
		aktifKullanici.HastaneYonetebilmeli(poliklinik.HastaneId);

		if (await doktorRepository.AnyAsync(d => d.PoliklinikId == id, ct) ||
			await saglikPersoneliRepository.AnyAsync(s => s.PoliklinikId == id, ct) ||
			await hizmetRepository.AnyAsync(h => h.PoliklinikId == id, ct))
			throw new IsKuraliException("Personeli veya hizmeti bulunan bir poliklinik silinemez.");

		poliklinikRepository.Remove(poliklinik);
		await unitOfWork.SaveChangesAsync(ct);
	}

	private async Task AdBenzersizOlmaliAsync(int hastaneId, string ad, int? haricId, CancellationToken ct)
	{
		if (await poliklinikRepository.AnyAsync(p => p.HastaneId == hastaneId && p.Ad == ad && p.Id != haricId, ct))
			throw new IsKuraliException($"Bu hastanede '{ad}' adında bir poliklinik zaten var.");
	}
}
