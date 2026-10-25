using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Core.UnitOfWorks;

namespace HastaneOtomasyon.Service.Services;

// Geliştirme ortamında projenin hemen denenebilmesi için örnek kurum ve hesaplar.
// Her adım "varsa kullan, yoksa oluştur" mantığıyla çalışır; uygulama her açılışta güvenle tekrar çağırabilir.
// Örnek hesapların ortak şifresi: Admin123
internal sealed class DemoVeriTohumlayici(
	IHastaneRepository hastaneRepository,
	IGenericRepository<CalismaSaati> calismaSaatiRepository,
	IGenericRepository<Poliklinik> poliklinikRepository,
	IHizmetRepository hizmetRepository,
	IKisiRepository kisiRepository,
	IDoktorRepository doktorRepository,
	IMusaitlikRepository musaitlikRepository,
	IRandevuRepository randevuRepository,
	ISifreHasher sifreHasher,
	IUnitOfWork unitOfWork,
	TimeProvider zaman)
{
	public const string Sifre = "Admin123";

	private static readonly DayOfWeek[] HaftaIci =
		[DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

	public async Task TohumlaAsync(CancellationToken ct = default)
	{
		var hastane = await HastaneAsync(ct);

		var kardiyoloji = await PoliklinikAsync(hastane.Id, "Kardiyoloji", "Kalp ve damar hastalıklarının tanı ve tedavisi.", ct);
		var dahiliye = await PoliklinikAsync(hastane.Id, "Dahiliye", "İç hastalıkları; genel sağlık kontrolü ve kronik hastalık takibi.", ct);

		var muayene = await HizmetAsync(kardiyoloji.Id, "Kardiyoloji muayenesi", 20, 1000, ct);
		var ekg = await HizmetAsync(kardiyoloji.Id, "EKG", 15, 450, ct);
		await HizmetAsync(dahiliye.Id, "Dahiliye muayenesi", 20, 700, ct);
		await HizmetAsync(dahiliye.Id, "Kan tahlili", 10, 300, ct);

		await KisiAsync(new Yonetici
		{
			TcKimlikNo = "20000000001",
			Ad = "Zeynep",
			Soyad = "Aydın",
			Email = "yonetici@mail.com",
			Telefon = "0532 000 00 01",
			HastaneId = hastane.Id,
			IseBaslamaTarihi = Bugun
		}, ct);

		var doktor = await KisiAsync(new Doktor
		{
			TcKimlikNo = "20000000002",
			Ad = "Mehmet",
			Soyad = "Kaya",
			Email = "doktor@mail.com",
			Telefon = "0532 000 00 02",
			HastaneId = hastane.Id,
			IseBaslamaTarihi = Bugun,
			PoliklinikId = kardiyoloji.Id,
			UzmanlikAlani = "Kardiyoloji",
			Unvan = "Uzm. Dr."
		}, ct);
		if (doktor is not null)
			await DoktoruHazirlaAsync(doktor, [muayene, ekg], ct);

		var hasta = await KisiAsync(new Hasta
		{
			TcKimlikNo = "20000000003",
			Ad = "Elif",
			Soyad = "Şahin",
			Email = "hasta@mail.com",
			Telefon = "0532 000 00 03",
			DogumTarihi = new DateTime(1995, 5, 14),
			Cinsiyet = "Kadın",
			KanGrubu = "A Rh+"
		}, ct);

		// Örnek randevular yalnızca hasta ve doktor bu çağrıda birlikte oluşturulduysa eklenir.
		if (hasta is not null && doktor is not null)
			await OrnekRandevularAsync(hasta, doktor, muayene, ekg, ct);
	}

	private DateTime Simdi => zaman.GetLocalNow().DateTime;
	private DateTime Bugun => Simdi.Date;

	private async Task<Hastane> HastaneAsync(CancellationToken ct)
	{
		const string ad = "Sakarya Merkez Hastanesi";
		var hastane = (await hastaneRepository.GetAllAsync(h => h.Ad == ad, ct)).FirstOrDefault();
		if (hastane is null)
		{
			hastane = new Hastane { Ad = ad, Adres = "Sağlık Cad. No: 12, Adapazarı / Sakarya", Telefon = "0264 111 22 33" };
			await hastaneRepository.AddAsync(hastane, ct);
			await unitOfWork.SaveChangesAsync(ct);
		}

		// Çalışma saati hiç tanımlı değilse hafta içi 08:00–20:00, cumartesi 09:00–14:00.
		if (!await calismaSaatiRepository.AnyAsync(c => c.HastaneId == hastane.Id, ct))
		{
			var saatler = HaftaIci
				.Select(gun => new CalismaSaati { HastaneId = hastane.Id, Gun = gun, Baslangic = new(8, 0, 0), Bitis = new(20, 0, 0) })
				.Append(new CalismaSaati { HastaneId = hastane.Id, Gun = DayOfWeek.Saturday, Baslangic = new(9, 0, 0), Bitis = new(14, 0, 0) });
			await calismaSaatiRepository.AddRangeAsync(saatler, ct);
			await unitOfWork.SaveChangesAsync(ct);
		}

		return hastane;
	}

	private async Task<Poliklinik> PoliklinikAsync(int hastaneId, string ad, string aciklama, CancellationToken ct)
	{
		var poliklinik = (await poliklinikRepository.GetAllAsync(p => p.HastaneId == hastaneId && p.Ad == ad, ct)).FirstOrDefault();
		if (poliklinik is not null)
			return poliklinik;

		poliklinik = new Poliklinik { HastaneId = hastaneId, Ad = ad, Aciklama = aciklama };
		await poliklinikRepository.AddAsync(poliklinik, ct);
		await unitOfWork.SaveChangesAsync(ct);
		return poliklinik;
	}

	private async Task<Hizmet> HizmetAsync(int poliklinikId, string ad, int sureDakika, decimal ucret, CancellationToken ct)
	{
		var hizmet = (await hizmetRepository.GetAllAsync(h => h.PoliklinikId == poliklinikId && h.Ad == ad, ct)).FirstOrDefault();
		if (hizmet is not null)
			return hizmet;

		hizmet = new Hizmet { PoliklinikId = poliklinikId, Ad = ad, SureDakika = sureDakika, Ucret = ucret };
		await hizmetRepository.AddAsync(hizmet, ct);
		await unitOfWork.SaveChangesAsync(ct);
		return hizmet;
	}

	// E-posta veya TC kimlik numarası kullanılıyorsa hesap oluşturulmaz ve null döner.
	private async Task<T?> KisiAsync<T>(T kisi, CancellationToken ct) where T : Kisi
	{
		if (await kisiRepository.AnyAsync(k => k.Email == kisi.Email || k.TcKimlikNo == kisi.TcKimlikNo, ct))
			return null;

		kisi.SifreHash = sifreHasher.Hashle(Sifre);
		await kisiRepository.AddAsync(kisi, ct);
		await unitOfWork.SaveChangesAsync(ct);
		return kisi;
	}

	// Hizmetleri atanır; hafta içi 09:00–12:00 ve 13:00–17:00 müsaittir.
	private async Task DoktoruHazirlaAsync(Doktor doktor, Hizmet[] hizmetler, CancellationToken ct)
	{
		foreach (var hizmet in hizmetler)
			await doktorRepository.DoktorHizmetEkleAsync(new DoktorHizmet { DoktorId = doktor.Id, HizmetId = hizmet.Id }, ct);

		var musaitlikler = HaftaIci.SelectMany(gun => new[]
		{
			new Musaitlik { DoktorId = doktor.Id, Gun = gun, Baslangic = new(9, 0, 0), Bitis = new(12, 0, 0) },
			new Musaitlik { DoktorId = doktor.Id, Gun = gun, Baslangic = new(13, 0, 0), Bitis = new(17, 0, 0) }
		});
		await musaitlikRepository.AddRangeAsync(musaitlikler, ct);
		await unitOfWork.SaveChangesAsync(ct);
	}

	// Sonraki ilk iki iş gününe biri onay bekleyen, biri onaylanmış iki randevu.
	private async Task OrnekRandevularAsync(Hasta hasta, Doktor doktor, Hizmet muayene, Hizmet ekg, CancellationToken ct)
	{
		var isGunleri = Enumerable.Range(1, 14)
			.Select(i => Bugun.AddDays(i))
			.Where(g => HaftaIci.Contains(g.DayOfWeek))
			.Take(2)
			.ToArray();

		var bekleyen = Randevu.Olustur(hasta.Id, doktor.Id, muayene, muayene.Ucret, isGunleri[0].AddHours(10),
			"Son günlerde çarpıntı şikayetim var.");
		var onayli = Randevu.Olustur(hasta.Id, doktor.Id, ekg, ekg.Ucret, isGunleri[1].AddHours(11), null);
		onayli.Onayla(doktor.Id, Simdi);

		await randevuRepository.AddRangeAsync([bekleyen, onayli], ct);
		await unitOfWork.SaveChangesAsync(ct);
	}
}
