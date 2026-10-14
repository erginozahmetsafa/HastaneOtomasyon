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

internal sealed class MusaitlikService(
	IMusaitlikRepository musaitlikRepository,
	IDoktorRepository doktorRepository,
	IGenericRepository<CalismaSaati> calismaSaatiRepository,
	IUnitOfWork unitOfWork,
	IAktifKullanici aktifKullanici) : IMusaitlikService
{
	public async Task<IReadOnlyList<MusaitlikDto>> GetByDoktorAsync(int doktorId, CancellationToken ct = default) =>
		(await musaitlikRepository.GetByDoktorAsync(doktorId, ct)).Select(m => m.ToDto()).ToList();

	public async Task<MusaitlikDto> EkleAsync(int doktorId, ZamanAraligiDto dto, CancellationToken ct = default)
	{
		var doktor = await doktorRepository.GetByIdOrThrowAsync(doktorId, ct);
		aktifKullanici.DoktorYonetebilmeli(doktor);
		dto.GecerliOlmali();

		var calismaSaatiIcinde = await calismaSaatiRepository.AnyAsync(c =>
			c.HastaneId == doktor.HastaneId &&
			c.Gun == dto.Gun &&
			c.Baslangic <= dto.Baslangic &&
			dto.Bitis <= c.Bitis, ct);
		if (!calismaSaatiIcinde)
			throw new IsKuraliException("Müsaitlik, hastanenin o günkü çalışma saatleri içinde olmalıdır.");

		var cakisiyor = await musaitlikRepository.AnyAsync(m =>
			m.DoktorId == doktorId &&
			m.Gun == dto.Gun &&
			m.Baslangic < dto.Bitis &&
			dto.Baslangic < m.Bitis, ct);
		if (cakisiyor)
			throw new IsKuraliException("Bu aralık doktorun mevcut bir müsaitlik dilimiyle çakışıyor.");

		var musaitlik = new Musaitlik { DoktorId = doktorId, Gun = dto.Gun, Baslangic = dto.Baslangic, Bitis = dto.Bitis };
		await musaitlikRepository.AddAsync(musaitlik, ct);
		await unitOfWork.SaveChangesAsync(ct);

		return musaitlik.ToDto();
	}

	// Mevcut randevular etkilenmez; yalnızca yeni randevu alınması engellenir.
	public async Task SilAsync(int musaitlikId, CancellationToken ct = default)
	{
		var musaitlik = await musaitlikRepository.GetByIdOrThrowAsync(musaitlikId, ct);
		var doktor = await doktorRepository.GetByIdOrThrowAsync(musaitlik.DoktorId, ct);
		aktifKullanici.DoktorYonetebilmeli(doktor);

		musaitlikRepository.Remove(musaitlik);
		await unitOfWork.SaveChangesAsync(ct);
	}
}
