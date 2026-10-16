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

internal sealed class RandevuService(
	IRandevuRepository randevuRepository,
	IDoktorRepository doktorRepository,
	IMusaitlikRepository musaitlikRepository,
	IUnitOfWork unitOfWork,
	IAktifKullanici aktifKullanici,
	TimeProvider zaman) : IRandevuService
{
	private DateTime Simdi => zaman.GetLocalNow().DateTime;

	public async Task<IReadOnlyList<DateTime>> UygunSaatleriGetirAsync(int doktorId, int hizmetId, DateOnly tarih, CancellationToken ct = default)
	{
		var hizmet = await SunulanHizmetiGetirAsync(doktorId, hizmetId, ct);
		var sure = TimeSpan.FromMinutes(hizmet.Hizmet.SureDakika);
		var gunBaslangici = tarih.ToDateTime(TimeOnly.MinValue);
		var gunSonu = gunBaslangici.AddDays(1);

		var musaitlikler = (await musaitlikRepository.GetByDoktorAsync(doktorId, ct))
			.Where(m => m.AktifMi && m.Gun == tarih.DayOfWeek);

		var doluAraliklar = await randevuRepository.GetAllAsync(r =>
			r.DoktorId == doktorId &&
			(r.Durum == RandevuDurum.Bekliyor || r.Durum == RandevuDurum.Onaylandi) &&
			r.Baslangic < gunSonu &&
			gunBaslangici < r.Bitis, ct);

		var simdi = Simdi;
		var uygunSaatler = new List<DateTime>();

		// Her müsaitlik dilimi, hizmet süresi kadar adımlarla dilimlere bölünür.
		foreach (var musaitlik in musaitlikler)
		{
			for (var saat = musaitlik.Baslangic; saat + sure <= musaitlik.Bitis; saat += sure)
			{
				var baslangic = gunBaslangici + saat;
				var bitis = baslangic + sure;

				if (baslangic > simdi && !doluAraliklar.Any(r => r.Baslangic < bitis && baslangic < r.Bitis))
					uygunSaatler.Add(baslangic);
			}
		}

		uygunSaatler.Sort();
		return uygunSaatler;
	}

	public async Task<RandevuDto> OlusturAsync(RandevuOlusturDto dto, CancellationToken ct = default)
	{
		var hastaId = aktifKullanici.RolOlmali(Roller.Hasta);

		if (dto.Baslangic <= Simdi)
			throw new IsKuraliException("Geçmiş bir zamana randevu alınamaz.");

		var doktorHizmet = await SunulanHizmetiGetirAsync(dto.DoktorId, dto.HizmetId, ct);
		var randevu = Randevu.Olustur(
			hastaId,
			dto.DoktorId,
			doktorHizmet.Hizmet,
			doktorHizmet.OzelUcret ?? doktorHizmet.Hizmet.Ucret,
			dto.Baslangic,
			dto.HastaNotu?.Trim());

		if (randevu.Bitis.Date != randevu.Baslangic.Date)
			throw new IsKuraliException("Randevu aynı gün içinde bitmelidir.");

		if (!await musaitlikRepository.MusaitMiAsync(randevu.DoktorId, randevu.Baslangic.DayOfWeek, randevu.Baslangic.TimeOfDay, randevu.Bitis.TimeOfDay, ct))
			throw new IsKuraliException("Doktor seçilen saatte müsait değil.");

		// Aynı doktora veya aynı hastaya gelen eşzamanlı istekler sırayla işlenir;
		// çakışma kontrolü ile kayıt arasına başka bir randevu giremez.
		string[] kilitler = [$"randevu:doktor:{randevu.DoktorId}", $"randevu:hasta:{hastaId}"];
		await unitOfWork.KilitliIslemAsync(kilitler, async () =>
		{
			if (await randevuRepository.DoktorCakismaVarMiAsync(randevu.DoktorId, randevu.Baslangic, randevu.Bitis, ct: ct))
				throw new IsKuraliException("Doktorun bu saatte başka bir randevusu var.");

			if (await randevuRepository.HastaCakismaVarMiAsync(hastaId, randevu.Baslangic, randevu.Bitis, ct: ct))
				throw new IsKuraliException("Bu saatte başka bir randevunuz var.");

			await randevuRepository.AddAsync(randevu, ct);
			await unitOfWork.SaveChangesAsync(ct);
		}, ct);

		return (await randevuRepository.GetDetayAsync(randevu.Id, ct))!.ToDto();
	}

	public async Task<IReadOnlyList<RandevuDto>> GetBenimRandevularimAsync(CancellationToken ct = default)
	{
		var hastaId = aktifKullanici.RolOlmali(Roller.Hasta);
		return (await randevuRepository.GetByHastaAsync(hastaId, ct)).Select(r => r.ToDto()).ToList();
	}

	public async Task<RandevuDto> GetDetayAsync(int id, CancellationToken ct = default)
	{
		var randevu = await randevuRepository.GetDetayAsync(id, ct) ?? throw BulunamadiException.Kayit<Randevu>(id);
		RandevuyaErisebilmeli(randevu, hastaDahil: true);
		return randevu.ToDto();
	}

	public Task IptalEtAsync(int id, string? neden, CancellationToken ct = default) =>
		DurumDegistirAsync(id, hastaDahil: true, (r, _) => r.IptalEt(neden, Simdi), ct);

	public Task OnaylaAsync(int id, CancellationToken ct = default) =>
		DurumDegistirAsync(id, hastaDahil: false, (r, kullaniciId) => r.Onayla(kullaniciId, Simdi), ct);

	public Task ReddetAsync(int id, string neden, CancellationToken ct = default) =>
		DurumDegistirAsync(id, hastaDahil: false, (r, kullaniciId) => r.Reddet(kullaniciId, neden, Simdi), ct);

	public Task TamamlaAsync(int id, CancellationToken ct = default) =>
		DurumDegistirAsync(id, hastaDahil: false, (r, _) => r.Tamamla(Simdi), ct);

	public async Task<IReadOnlyList<RandevuDto>> GetDoktorTakvimiAsync(int doktorId, DateOnly baslangic, DateOnly bitis, CancellationToken ct = default)
	{
		if (bitis < baslangic)
			throw new IsKuraliException("Bitiş tarihi başlangıç tarihinden önce olamaz.");

		var doktor = await doktorRepository.GetByIdOrThrowAsync(doktorId, ct);
		aktifKullanici.DoktorYonetebilmeli(doktor);

		var randevular = await randevuRepository.GetByDoktorAsync(
			doktorId,
			baslangic.ToDateTime(TimeOnly.MinValue),
			bitis.AddDays(1).ToDateTime(TimeOnly.MinValue),
			ct);

		return randevular.Select(r => r.ToDto()).ToList();
	}

	public async Task<IReadOnlyList<RandevuDto>> GetOnayBekleyenlerAsync(int hastaneId, CancellationToken ct = default)
	{
		aktifKullanici.HastaneYonetebilmeli(hastaneId);
		return (await randevuRepository.GetOnayBekleyenlerAsync(hastaneId, ct)).Select(r => r.ToDto()).ToList();
	}

	// Durum geçişlerinin ortak akışı: kaydı getir → yetki → entity'deki kural metodu → kaydet.
	private async Task DurumDegistirAsync(int id, bool hastaDahil, Action<Randevu, int> islem, CancellationToken ct)
	{
		var kullaniciId = aktifKullanici.GirisYapmisOlmali();
		var randevu = await randevuRepository.GetIslemIcinAsync(id, ct) ?? throw BulunamadiException.Kayit<Randevu>(id);

		RandevuyaErisebilmeli(randevu, hastaDahil);
		islem(randevu, kullaniciId);

		await unitOfWork.SaveChangesAsync(ct);
	}

	// Randevunun doktoru veya hastane yöneticisi; hastaDahil ise randevunun hastası da.
	private void RandevuyaErisebilmeli(Randevu randevu, bool hastaDahil)
	{
		aktifKullanici.GirisYapmisOlmali();

		var erisebilir = aktifKullanici.Rol switch
		{
			Roller.Hasta => hastaDahil && aktifKullanici.Id == randevu.HastaId,
			Roller.Doktor => aktifKullanici.Id == randevu.DoktorId,
			_ => aktifKullanici.HastaneYoneticisiMi(randevu.Doktor.HastaneId)
		};

		if (!erisebilir)
			throw new YetkisizIslemException();
	}

	// Doktor bu hizmeti sunmuyorsa, doktor pasifse veya hizmet pasifse randevu alınamaz.
	private async Task<DoktorHizmet> SunulanHizmetiGetirAsync(int doktorId, int hizmetId, CancellationToken ct)
	{
		var doktorHizmet = await doktorRepository.GetDoktorHizmetAsync(doktorId, hizmetId, ct)
			?? throw new IsKuraliException("Seçilen doktor bu hizmeti sunmuyor.");

		if (!doktorHizmet.Doktor.AktifMi)
			throw new IsKuraliException("Seçilen doktor şu anda randevu kabul etmiyor.");

		if (!doktorHizmet.Hizmet.AktifMi)
			throw new IsKuraliException("Seçilen hizmet şu anda verilmiyor.");

		return doktorHizmet;
	}
}
