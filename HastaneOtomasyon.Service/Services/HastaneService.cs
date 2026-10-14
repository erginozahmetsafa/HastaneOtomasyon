using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Core.UnitOfWorks;
using HastaneOtomasyon.Service.Extensions;
using HastaneOtomasyon.Service.Mapping;
using HastaneOtomasyon.Service.Security;

namespace HastaneOtomasyon.Service.Services;

internal sealed class HastaneService(
	IHastaneRepository hastaneRepository,
	IGenericRepository<Poliklinik> poliklinikRepository,
	IGenericRepository<Calisan> calisanRepository,
	IGenericRepository<CalismaSaati> calismaSaatiRepository,
	IUnitOfWork unitOfWork,
	IAktifKullanici aktifKullanici) : IHastaneService
{
	public async Task<IReadOnlyList<HastaneDto>> GetAllAsync(CancellationToken ct = default)
	{
		var hastaneler = await hastaneRepository.GetAllAsync(ct: ct);
		return hastaneler.OrderBy(h => h.Ad).Select(h => h.ToDto()).ToList();
	}

	public async Task<HastaneDetayDto> GetDetayAsync(int id, CancellationToken ct = default)
	{
		var hastane = await hastaneRepository.GetDetayAsync(id, ct) ?? throw BulunamadiException.Kayit<Hastane>(id);
		return hastane.ToDetayDto();
	}

	public async Task<HastaneDto> OlusturAsync(HastaneKaydetDto dto, CancellationToken ct = default)
	{
		aktifKullanici.RolOlmali(Roller.SistemYoneticisi);

		var hastane = new Hastane();
		BilgileriAktar(hastane, dto);

		await hastaneRepository.AddAsync(hastane, ct);
		await unitOfWork.SaveChangesAsync(ct);

		return hastane.ToDto();
	}

	public async Task GuncelleAsync(int id, HastaneKaydetDto dto, CancellationToken ct = default)
	{
		aktifKullanici.HastaneYonetebilmeli(id);

		var hastane = await hastaneRepository.GetByIdOrThrowAsync(id, ct);
		BilgileriAktar(hastane, dto);

		await unitOfWork.SaveChangesAsync(ct);
	}

	public async Task SilAsync(int id, CancellationToken ct = default)
	{
		aktifKullanici.RolOlmali(Roller.SistemYoneticisi);

		var hastane = await hastaneRepository.GetByIdOrThrowAsync(id, ct);

		if (await poliklinikRepository.AnyAsync(p => p.HastaneId == id, ct) ||
			await calisanRepository.AnyAsync(c => c.HastaneId == id, ct))
			throw new IsKuraliException("Polikliniği veya çalışanı bulunan bir hastane silinemez.");

		hastaneRepository.Remove(hastane);
		await unitOfWork.SaveChangesAsync(ct);
	}

	public async Task<CalismaSaatiDto> CalismaSaatiEkleAsync(int hastaneId, ZamanAraligiDto dto, CancellationToken ct = default)
	{
		aktifKullanici.HastaneYonetebilmeli(hastaneId);
		dto.GecerliOlmali();
		await hastaneRepository.VarOlmaliAsync(hastaneId, ct);

		var cakisiyor = await calismaSaatiRepository.AnyAsync(c =>
			c.HastaneId == hastaneId &&
			c.Gun == dto.Gun &&
			c.Baslangic < dto.Bitis &&
			dto.Baslangic < c.Bitis, ct);
		if (cakisiyor)
			throw new IsKuraliException("Bu gün için çakışan bir çalışma saati zaten tanımlı.");

		var calismaSaati = new CalismaSaati { HastaneId = hastaneId, Gun = dto.Gun, Baslangic = dto.Baslangic, Bitis = dto.Bitis };
		await calismaSaatiRepository.AddAsync(calismaSaati, ct);
		await unitOfWork.SaveChangesAsync(ct);

		return calismaSaati.ToDto();
	}

	public async Task CalismaSaatiSilAsync(int calismaSaatiId, CancellationToken ct = default)
	{
		var calismaSaati = await calismaSaatiRepository.GetByIdOrThrowAsync(calismaSaatiId, ct);
		aktifKullanici.HastaneYonetebilmeli(calismaSaati.HastaneId);

		calismaSaatiRepository.Remove(calismaSaati);
		await unitOfWork.SaveChangesAsync(ct);
	}

	private static void BilgileriAktar(Hastane hastane, HastaneKaydetDto dto)
	{
		hastane.Ad = dto.Ad.Trim();
		hastane.Adres = dto.Adres.Trim();
		hastane.Telefon = dto.Telefon?.Trim();
	}
}
