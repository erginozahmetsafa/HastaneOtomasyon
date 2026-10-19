using System.Linq.Expressions;
using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Core.UnitOfWorks;
using HastaneOtomasyon.Service.Services;
using HastaneOtomasyon.UnitTests.Yardimcilar;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace HastaneOtomasyon.UnitTests.Services;

public class RandevuServiceTests
{
	private const int HastaId = 5;
	private const int DoktorId = 10;
	private const int HizmetId = 1;

	private static readonly DateTime Simdi = TestVerisi.Simdi; // Pazartesi 10:00
	private static readonly DateTime Yarin = Simdi.Date.AddDays(1);

	private readonly Mock<IRandevuRepository> _randevuRepository = new();
	private readonly Mock<IDoktorRepository> _doktorRepository = new();
	private readonly Mock<IMusaitlikRepository> _musaitlikRepository = new();
	private readonly Mock<IUnitOfWork> _unitOfWork = new();
	private readonly FakeTimeProvider _zaman = new(new DateTimeOffset(Simdi, TimeSpan.Zero));

	private readonly Doktor _doktor = TestVerisi.Doktor(DoktorId);
	private readonly Hizmet _hizmet = TestVerisi.Hizmet(HizmetId, sureDakika: 30, ucret: 500m);
	private readonly List<Randevu> _randevular = [];
	private IReadOnlyList<string> _alinanKilitler = [];

	public RandevuServiceTests()
	{
		_doktorRepository
			.Setup(r => r.GetDoktorHizmetAsync(DoktorId, HizmetId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => TestVerisi.DoktorHizmet(_doktor, _hizmet));

		// Filtre ifadesi bellekteki listeye uygulanır; böylece servisin yazdığı sorgu da sınanmış olur.
		_randevuRepository
			.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Randevu, bool>>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((Expression<Func<Randevu, bool>>? filtre, CancellationToken _) =>
				_randevular.Where(filtre!.Compile()).ToList());

		_musaitlikRepository
			.Setup(r => r.MusaitMiAsync(DoktorId, It.IsAny<DayOfWeek>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Gerçek gerçekleştirim bir veritabanı kilidi alır; burada işlem doğrudan çalıştırılır.
		_unitOfWork
			.Setup(u => u.KilitliIslemAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
			.Returns((IEnumerable<string> kaynaklar, Func<Task> islem, CancellationToken _) =>
			{
				_alinanKilitler = kaynaklar.ToList();
				return islem();
			});
	}

	private RandevuService Servis(IAktifKullanici kullanici) => new(
		_randevuRepository.Object,
		_doktorRepository.Object,
		_musaitlikRepository.Object,
		_unitOfWork.Object,
		kullanici,
		_zaman);

	private void RandevuVar(Randevu randevu) =>
		_randevuRepository
			.Setup(r => r.GetIslemIcinAsync(randevu.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(randevu);

	private static RandevuOlusturDto OlusturDto(DateTime baslangic) => new()
	{
		DoktorId = DoktorId,
		HizmetId = HizmetId,
		Baslangic = baslangic,
		HastaNotu = "  Kontrol  "
	};

	// ---- Uygun saatler ----

	[Fact]
	public async Task UygunSaatler_MusaitlikHizmetSuresineBolunur_GecmisVeDoluSaatlerCikarilir()
	{
		_musaitlikRepository
			.Setup(r => r.GetByDoktorAsync(DoktorId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(
			[
				TestVerisi.Musaitlik(DayOfWeek.Monday, 14, 15),
				TestVerisi.Musaitlik(DayOfWeek.Monday, 9, 12),
				TestVerisi.Musaitlik(DayOfWeek.Monday, 16, 18, aktifMi: false), // pasif dilim
				TestVerisi.Musaitlik(DayOfWeek.Tuesday, 9, 12)                  // başka gün
			]);

		_randevular.AddRange(
		[
			TestVerisi.Randevu(Simdi.Date.AddHours(10.5), RandevuDurum.Onaylandi),                // 10:30 dolu
			TestVerisi.Randevu(Simdi.Date.AddHours(11), RandevuDurum.IptalEdildi),                // iptal: yer kaplamaz
			TestVerisi.Randevu(Simdi.Date.AddHours(14), doktor: TestVerisi.Doktor(id: 99))        // başka doktor
		]);

		var saatler = await Servis(TestKullanici.HastaOlarak(HastaId)).UygunSaatleriGetirAsync(DoktorId, HizmetId, TestVerisi.Bugun);

		// 09:00-10:00 geçmiş (10:00 dahil), 10:30 dolu; sonuç saate göre sıralı.
		DateTime[] beklenen = [.. new[] { 11, 11.5, 14, 14.5 }.Select(s => Simdi.Date.AddHours(s))];
		Assert.Equal(beklenen, saatler);
	}

	[Fact]
	public async Task UygunSaatler_DilimSonundaHizmetSuresineYetmeyenKisimKullanilmaz()
	{
		var uzunHizmet = TestVerisi.Hizmet(HizmetId, sureDakika: 50);
		_doktorRepository
			.Setup(r => r.GetDoktorHizmetAsync(DoktorId, HizmetId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(TestVerisi.DoktorHizmet(_doktor, uzunHizmet));
		_musaitlikRepository
			.Setup(r => r.GetByDoktorAsync(DoktorId, It.IsAny<CancellationToken>()))
			.ReturnsAsync([TestVerisi.Musaitlik(DayOfWeek.Tuesday, 9, 11)]);

		var saatler = await Servis(TestKullanici.HastaOlarak(HastaId)).UygunSaatleriGetirAsync(DoktorId, HizmetId, TestVerisi.Bugun.AddDays(1));

		// 09:00 ve 09:50 sığar; 10:40 + 50 dk = 11:30 dilimi aşar.
		DateTime[] beklenen = [Yarin.AddHours(9), Yarin.AddHours(9).AddMinutes(50)];
		Assert.Equal(beklenen, saatler);
	}

	[Fact]
	public async Task UygunSaatler_DoktorHizmetiSunmuyor_HataVerir()
	{
		var hata = await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.HastaOlarak(HastaId)).UygunSaatleriGetirAsync(DoktorId, hizmetId: 999, TestVerisi.Bugun));

		Assert.Contains("hizmeti sunmuyor", hata.Message);
	}

	[Fact]
	public async Task UygunSaatler_DoktorPasif_HataVerir()
	{
		_doktor.AktifMi = false;

		await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.HastaOlarak(HastaId)).UygunSaatleriGetirAsync(DoktorId, HizmetId, TestVerisi.Bugun));
	}

	[Fact]
	public async Task UygunSaatler_HizmetPasif_HataVerir()
	{
		_hizmet.AktifMi = false;

		await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.HastaOlarak(HastaId)).UygunSaatleriGetirAsync(DoktorId, HizmetId, TestVerisi.Bugun));
	}

	// ---- Randevu oluşturma ----

	[Fact]
	public async Task Olustur_GecerliIstek_KilitAltindaKaydedilir()
	{
		var baslangic = Yarin.AddHours(9);
		_doktorRepository
			.Setup(r => r.GetDoktorHizmetAsync(DoktorId, HizmetId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(TestVerisi.DoktorHizmet(_doktor, _hizmet, ozelUcret: 650m));

		Randevu? eklenen = null;
		_randevuRepository
			.Setup(r => r.AddAsync(It.IsAny<Randevu>(), It.IsAny<CancellationToken>()))
			.Callback((Randevu r, CancellationToken _) => eklenen = r);
		_randevuRepository
			.Setup(r => r.GetDetayAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => TestVerisi.Randevu(eklenen!.Baslangic, doktor: _doktor, hizmet: _hizmet));

		await Servis(TestKullanici.HastaOlarak(HastaId)).OlusturAsync(OlusturDto(baslangic));

		Assert.NotNull(eklenen);
		Assert.Equal(HastaId, eklenen.HastaId);           // hasta Id'si token'dan gelir
		Assert.Equal(baslangic.AddMinutes(30), eklenen.Bitis);
		Assert.Equal(650m, eklenen.Ucret);                // doktora özel ücret önceliklidir
		Assert.Equal("Kontrol", eklenen.HastaNotu);
		Assert.Equal(RandevuDurum.Bekliyor, eklenen.Durum);
		string[] beklenenKilitler = ["randevu:doktor:10", "randevu:hasta:5"];
		Assert.Equal(beklenenKilitler, _alinanKilitler);
		_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Olustur_OzelUcretYoksa_HizmetUcretiKullanilir()
	{
		Randevu? eklenen = null;
		_randevuRepository
			.Setup(r => r.AddAsync(It.IsAny<Randevu>(), It.IsAny<CancellationToken>()))
			.Callback((Randevu r, CancellationToken _) => eklenen = r);
		_randevuRepository
			.Setup(r => r.GetDetayAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => TestVerisi.Randevu(eklenen!.Baslangic));

		await Servis(TestKullanici.HastaOlarak(HastaId)).OlusturAsync(OlusturDto(Yarin.AddHours(9)));

		Assert.Equal(500m, eklenen!.Ucret);
	}

	public static TheoryData<TestKullanici> HastaOlmayanlar => new()
	{
		TestKullanici.Anonim,
		TestKullanici.DoktorOlarak(DoktorId, TestVerisi.HastaneId),
		TestKullanici.YoneticiOlarak(2, TestVerisi.HastaneId),
		TestKullanici.SistemYoneticisiOlarak()
	};

	[Theory]
	[MemberData(nameof(HastaOlmayanlar))]
	public async Task Olustur_YalnizcaHastaRandevuAlabilir(TestKullanici kullanici)
	{
		await Assert.ThrowsAsync<YetkisizIslemException>(() => Servis(kullanici).OlusturAsync(OlusturDto(Yarin.AddHours(9))));
	}

	[Fact]
	public async Task Olustur_GecmisZaman_HataVerir()
	{
		await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.HastaOlarak(HastaId)).OlusturAsync(OlusturDto(Simdi.AddMinutes(-30))));
	}

	[Fact]
	public async Task Olustur_ErtesiGuneTasanRandevu_HataVerir()
	{
		var hata = await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.HastaOlarak(HastaId)).OlusturAsync(OlusturDto(Yarin.AddHours(23).AddMinutes(45))));

		Assert.Contains("aynı gün", hata.Message);
	}

	[Fact]
	public async Task Olustur_DoktorMusaitDegil_HataVerir()
	{
		_musaitlikRepository
			.Setup(r => r.MusaitMiAsync(DoktorId, It.IsAny<DayOfWeek>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.HastaOlarak(HastaId)).OlusturAsync(OlusturDto(Yarin.AddHours(9))));

		_unitOfWork.Verify(u => u.KilitliIslemAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Olustur_DoktorunBaskaRandevusuyleCakisiyor_HataVerirVeKaydetmez()
	{
		_randevuRepository
			.Setup(r => r.DoktorCakismaVarMiAsync(DoktorId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		var hata = await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.HastaOlarak(HastaId)).OlusturAsync(OlusturDto(Yarin.AddHours(9))));

		Assert.Contains("Doktorun", hata.Message);
		_randevuRepository.Verify(r => r.AddAsync(It.IsAny<Randevu>(), It.IsAny<CancellationToken>()), Times.Never);
		_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Olustur_HastaninBaskaRandevusuyleCakisiyor_HataVerirVeKaydetmez()
	{
		_randevuRepository
			.Setup(r => r.HastaCakismaVarMiAsync(HastaId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		var hata = await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.HastaOlarak(HastaId)).OlusturAsync(OlusturDto(Yarin.AddHours(9))));

		Assert.Contains("randevunuz var", hata.Message);
		_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	// ---- Onay mekanizması ve erişim ----

	public static TheoryData<TestKullanici> OnaylayabilecekKullanicilar => new()
	{
		TestKullanici.DoktorOlarak(DoktorId, TestVerisi.HastaneId),   // randevunun doktoru
		TestKullanici.YoneticiOlarak(2, TestVerisi.HastaneId),        // doktorun hastanesinin yöneticisi
		TestKullanici.SistemYoneticisiOlarak()
	};

	[Theory]
	[MemberData(nameof(OnaylayabilecekKullanicilar))]
	public async Task Onayla_YetkiliKullanici_OnaylanirVeKaydedilir(TestKullanici kullanici)
	{
		var randevu = TestVerisi.Randevu(Yarin.AddHours(9), doktor: _doktor);
		RandevuVar(randevu);

		await Servis(kullanici).OnaylaAsync(randevu.Id);

		Assert.Equal(RandevuDurum.Onaylandi, randevu.Durum);
		Assert.Equal(kullanici.Id, randevu.OnaylayanId);
		_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	public static TheoryData<TestKullanici> OnaylayamayacakKullanicilar => new()
	{
		TestKullanici.HastaOlarak(HastaId),                            // randevunun kendi hastası bile onaylayamaz
		TestKullanici.DoktorOlarak(11, TestVerisi.HastaneId),          // başka doktor
		TestKullanici.YoneticiOlarak(2, TestVerisi.DigerHastaneId),    // başka hastanenin yöneticisi
		TestKullanici.PersonelOlarak(4, TestVerisi.HastaneId)
	};

	[Theory]
	[MemberData(nameof(OnaylayamayacakKullanicilar))]
	public async Task Onayla_YetkisizKullanici_HataVerirVeDurumDegismez(TestKullanici kullanici)
	{
		var randevu = TestVerisi.Randevu(Yarin.AddHours(9), doktor: _doktor);
		RandevuVar(randevu);

		await Assert.ThrowsAsync<YetkisizIslemException>(() => Servis(kullanici).OnaylaAsync(randevu.Id));

		Assert.Equal(RandevuDurum.Bekliyor, randevu.Durum);
		_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Onayla_RandevuYok_BulunamadiHatasi()
	{
		await Assert.ThrowsAsync<BulunamadiException>(() =>
			Servis(TestKullanici.DoktorOlarak(DoktorId, TestVerisi.HastaneId)).OnaylaAsync(12345));
	}

	[Fact]
	public async Task IptalEt_HastaKendiRandevusunu_IptalEdebilir()
	{
		var randevu = TestVerisi.Randevu(Yarin.AddHours(9), hasta: TestVerisi.Hasta(HastaId), doktor: _doktor);
		RandevuVar(randevu);

		await Servis(TestKullanici.HastaOlarak(HastaId)).IptalEtAsync(randevu.Id, "Gelemeyeceğim");

		Assert.Equal(RandevuDurum.IptalEdildi, randevu.Durum);
	}

	[Fact]
	public async Task IptalEt_BaskaHastaninRandevusu_HataVerir()
	{
		var randevu = TestVerisi.Randevu(Yarin.AddHours(9), hasta: TestVerisi.Hasta(HastaId), doktor: _doktor);
		RandevuVar(randevu);

		await Assert.ThrowsAsync<YetkisizIslemException>(() =>
			Servis(TestKullanici.HastaOlarak(6)).IptalEtAsync(randevu.Id, null));

		Assert.Equal(RandevuDurum.Bekliyor, randevu.Durum);
	}

	[Fact]
	public async Task GetDetay_BaskaHastaninRandevusu_HataVerir()
	{
		var randevu = TestVerisi.Randevu(Yarin.AddHours(9), hasta: TestVerisi.Hasta(HastaId), doktor: _doktor);
		_randevuRepository.Setup(r => r.GetDetayAsync(randevu.Id, It.IsAny<CancellationToken>())).ReturnsAsync(randevu);

		await Assert.ThrowsAsync<YetkisizIslemException>(() => Servis(TestKullanici.HastaOlarak(6)).GetDetayAsync(randevu.Id));
	}

	[Fact]
	public async Task DoktorTakvimi_BitisBaslangictanOnce_HataVerir()
	{
		await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.DoktorOlarak(DoktorId, TestVerisi.HastaneId))
				.GetDoktorTakvimiAsync(DoktorId, TestVerisi.Bugun, TestVerisi.Bugun.AddDays(-1)));
	}
}
