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

internal sealed class PersonelService(
	IKisiRepository kisiRepository,
	IGenericRepository<Calisan> calisanRepository,
	IDoktorRepository doktorRepository,
	IHastaneRepository hastaneRepository,
	IGenericRepository<Poliklinik> poliklinikRepository,
	IHizmetRepository hizmetRepository,
	KisiFabrikasi kisiFabrikasi,
	IUnitOfWork unitOfWork,
	IAktifKullanici aktifKullanici,
	TimeProvider zaman) : IPersonelService
{
	public async Task<IReadOnlyList<DoktorDto>> GetDoktorlarAsync(int poliklinikId, CancellationToken ct = default) =>
		(await doktorRepository.GetByPoliklinikAsync(poliklinikId, ct)).Select(d => d.ToDto()).ToList();

	public async Task<IReadOnlyList<DoktorDto>> GetDoktorlarByHizmetAsync(int hizmetId, CancellationToken ct = default) =>
		(await doktorRepository.GetByHizmetAsync(hizmetId, ct)).Select(d => d.ToDto()).ToList();

	public async Task<DoktorDetayDto> GetDoktorDetayAsync(int doktorId, CancellationToken ct = default)
	{
		var doktor = await doktorRepository.GetDetayAsync(doktorId, ct) ?? throw BulunamadiException.Kayit<Doktor>(doktorId);
		return doktor.ToDetayDto();
	}

	public async Task<IReadOnlyList<PersonelDto>> GetPersonellerAsync(int hastaneId, CancellationToken ct = default)
	{
		aktifKullanici.HastaneYonetebilmeli(hastaneId);

		var calisanlar = await calisanRepository.GetAllAsync(c => c.HastaneId == hastaneId, ct);
		return calisanlar.OrderBy(c => c.Ad).ThenBy(c => c.Soyad).Select(c => c.ToPersonelDto()).ToList();
	}

	public async Task<PersonelDto> DoktorEkleAsync(DoktorKayitDto dto, CancellationToken ct = default)
	{
		var doktor = await CalisanOlusturAsync<Doktor>(dto, ct);
		await PoliklinikHastaneyeAitOlmaliAsync(dto.PoliklinikId, dto.HastaneId, ct);

		doktor.PoliklinikId = dto.PoliklinikId;
		doktor.UzmanlikAlani = dto.UzmanlikAlani.Trim();
		doktor.Unvan = dto.Unvan?.Trim();

		return await KaydetAsync(doktor, ct);
	}

	public async Task<PersonelDto> SaglikPersoneliEkleAsync(SaglikPersoneliKayitDto dto, CancellationToken ct = default)
	{
		var personel = await CalisanOlusturAsync<SaglikPersoneli>(dto, ct);
		if (dto.PoliklinikId is int poliklinikId)
			await PoliklinikHastaneyeAitOlmaliAsync(poliklinikId, dto.HastaneId, ct);

		personel.Gorev = dto.Gorev.Trim();
		personel.PoliklinikId = dto.PoliklinikId;

		return await KaydetAsync(personel, ct);
	}

	public async Task<PersonelDto> YoneticiEkleAsync(YoneticiKayitDto dto, CancellationToken ct = default)
	{
		aktifKullanici.RolOlmali(Roller.SistemYoneticisi);

		var yonetici = await CalisanOlusturAsync<Yonetici>(dto, ct);
		return await KaydetAsync(yonetici, ct);
	}

	public async Task DoktorGuncelleAsync(int doktorId, DoktorGuncelleDto dto, CancellationToken ct = default)
	{
		var doktor = await doktorRepository.GetByIdOrThrowAsync(doktorId, ct);
		aktifKullanici.HastaneYonetebilmeli(doktor.HastaneId);

		if (doktor.PoliklinikId != dto.PoliklinikId)
			await PoliklinikHastaneyeAitOlmaliAsync(dto.PoliklinikId, doktor.HastaneId, ct);

		doktor.PoliklinikId = dto.PoliklinikId;
		doktor.UzmanlikAlani = dto.UzmanlikAlani.Trim();
		doktor.Unvan = dto.Unvan?.Trim();

		await unitOfWork.SaveChangesAsync(ct);
	}

	public async Task DoktorHizmetAtaAsync(int doktorId, DoktorHizmetAtaDto dto, CancellationToken ct = default)
	{
		var doktor = await doktorRepository.GetByIdOrThrowAsync(doktorId, ct);
		aktifKullanici.HastaneYonetebilmeli(doktor.HastaneId);

		var uygunHizmet = await hizmetRepository.AnyAsync(h =>
			h.Id == dto.HizmetId &&
			h.AktifMi &&
			h.Poliklinik.HastaneId == doktor.HastaneId, ct);
		if (!uygunHizmet)
			throw new IsKuraliException("Hizmet bulunamadı, aktif değil veya doktorun hastanesine ait değil.");

		if (await doktorRepository.GetDoktorHizmetAsync(doktorId, dto.HizmetId, ct) is not null)
			throw new IsKuraliException("Bu hizmet doktora zaten atanmış.");

		await doktorRepository.DoktorHizmetEkleAsync(new DoktorHizmet
		{
			DoktorId = doktorId,
			HizmetId = dto.HizmetId,
			OzelUcret = dto.OzelUcret
		}, ct);
		await unitOfWork.SaveChangesAsync(ct);
	}

	public async Task DoktorHizmetKaldirAsync(int doktorId, int hizmetId, CancellationToken ct = default)
	{
		var doktor = await doktorRepository.GetByIdOrThrowAsync(doktorId, ct);
		aktifKullanici.HastaneYonetebilmeli(doktor.HastaneId);

		var doktorHizmet = await doktorRepository.GetDoktorHizmetAsync(doktorId, hizmetId, ct)
			?? throw new BulunamadiException("Bu hizmet doktora atanmamış.");

		doktorRepository.DoktorHizmetKaldir(doktorHizmet);
		await unitOfWork.SaveChangesAsync(ct);
	}

	// Tüm çalışan türleri için ortak adımlar: yetki, hastane kontrolü ve ortak alanlar.
	private async Task<T> CalisanOlusturAsync<T>(CalisanKayitDto dto, CancellationToken ct) where T : Calisan, new()
	{
		aktifKullanici.HastaneYonetebilmeli(dto.HastaneId);
		await hastaneRepository.VarOlmaliAsync(dto.HastaneId, ct);

		var calisan = await kisiFabrikasi.OlusturAsync<T>(dto, ct);
		calisan.HastaneId = dto.HastaneId;
		calisan.IseBaslamaTarihi = (dto.IseBaslamaTarihi ?? zaman.GetLocalNow().DateTime).Date;

		return calisan;
	}

	private async Task<PersonelDto> KaydetAsync(Calisan calisan, CancellationToken ct)
	{
		await kisiRepository.AddAsync(calisan, ct);
		await unitOfWork.SaveChangesAsync(ct);
		return calisan.ToPersonelDto();
	}

	private async Task PoliklinikHastaneyeAitOlmaliAsync(int poliklinikId, int hastaneId, CancellationToken ct)
	{
		if (!await poliklinikRepository.AnyAsync(p => p.Id == poliklinikId && p.HastaneId == hastaneId, ct))
			throw new IsKuraliException("Seçilen poliklinik bu hastaneye ait değil.");
	}
}
